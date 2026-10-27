import { useEffect, useRef, useState } from 'react'
import { HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr'
import type { PriceTicker } from '../types'
import { supabase } from '../supabase'

const PAIRS = ['BTCUSDT', 'ETHUSDT', 'BNBUSDT', 'SOLUSDT', 'XRPUSDT', 'ADAUSDT', 'DOGEUSDT', 'DOTUSDT']

const GATEWAY_URL = import.meta.env.VITE_GATEWAY_URL ?? 'http://localhost:8001'

export function usePrices() {
  const [prices, setPrices] = useState<Record<string, PriceTicker>>({})
  const connRef = useRef<ReturnType<typeof HubConnectionBuilder.prototype.build> | null>(null)

  useEffect(() => {
    let cancelled = false

    async function connect() {
      const { data } = await supabase.auth.getSession()
      const token = data.session?.access_token

      const conn = new HubConnectionBuilder()
        .withUrl(`${GATEWAY_URL}/hubs/prices`, {
          accessTokenFactory: () => token ?? ''
        })
        .withAutomaticReconnect()
        .build()

      connRef.current = conn

      conn.on('PriceUpdate', (ticker: PriceTicker) => {
        if (!cancelled) {
          setPrices(prev => ({ ...prev, [ticker.symbol]: ticker }))
        }
      })

      await conn.start()

      for (const pair of PAIRS) {
        await conn.invoke('SubscribeToPair', pair)
      }
    }

    connect().catch(console.error)

    return () => {
      cancelled = true
      const conn = connRef.current
      if (conn && conn.state === HubConnectionState.Connected) {
        conn.stop()
      }
    }
  }, [])

  return { prices, pairs: PAIRS }
}
