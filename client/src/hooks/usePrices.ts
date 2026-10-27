import { useEffect, useState } from 'react'
import { HubConnectionBuilder, HubConnectionState, type HubConnection } from '@microsoft/signalr'
import type { PriceTicker } from '../types'
import { supabase } from '../supabase'

const PAIRS = ['BTCUSDT', 'ETHUSDT', 'BNBUSDT', 'SOLUSDT', 'XRPUSDT', 'ADAUSDT', 'DOGEUSDT', 'DOTUSDT']
const GATEWAY_URL = import.meta.env.VITE_GATEWAY_URL ?? 'http://localhost:8001'
const MAX_HISTORY = 60

let sharedConn: HubConnection | null = null
let connecting = false
const listeners = new Set<(t: PriceTicker) => void>()
const priceHistory: Record<string, number[]> = {}

async function ensureConnection() {
  if (sharedConn?.state === HubConnectionState.Connected || connecting) return
  connecting = true
  try {
    const { data } = await supabase.auth.getSession()
    const token = data.session?.access_token
    if (!token) return

    const conn = new HubConnectionBuilder()
      .withUrl(`${GATEWAY_URL}/hubs/prices`, { accessTokenFactory: () => token })
      .withAutomaticReconnect()
      .build()

    conn.on('PriceUpdate', (ticker: PriceTicker) => {
      const hist = priceHistory[ticker.symbol] ??= []
      hist.push(ticker.lastPrice)
      if (hist.length > MAX_HISTORY) hist.shift()
      listeners.forEach(fn => fn(ticker))
    })

    await conn.start()
    sharedConn = conn

    for (const pair of PAIRS) {
      await conn.invoke('SubscribeToPair', pair)
    }
  } catch (err) {
    console.error('SignalR connection failed:', err)
  } finally {
    connecting = false
  }
}

export function usePrices() {
  const [prices, setPrices] = useState<Record<string, PriceTicker>>({})
  const [history, setHistory] = useState<Record<string, number[]>>({})

  useEffect(() => {
    const handler = (ticker: PriceTicker) => {
      setPrices(prev => ({ ...prev, [ticker.symbol]: ticker }))
      setHistory(() => {
        const snap: Record<string, number[]> = {}
        for (const k in priceHistory) snap[k] = [...priceHistory[k]]
        return snap
      })
    }
    listeners.add(handler)
    ensureConnection()
    return () => { listeners.delete(handler) }
  }, [])

  return { prices, history, pairs: PAIRS }
}
