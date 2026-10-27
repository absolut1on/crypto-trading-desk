import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { usePrices } from '../hooks/usePrices'
import { api } from '../services/api'
import { OrderSideLabel, type Trade, type WalletBalance, type Position } from '../types'
import Sparkline from '../components/Sparkline'

export default function Dashboard() {
  const { session } = useAuth()
  const { prices, history, pairs } = usePrices()
  const [balances, setBalances] = useState<WalletBalance[]>([])
  const [positions, setPositions] = useState<Position[]>([])
  const [trades, setTrades] = useState<Trade[]>([])

  useEffect(() => {
    api.getMe().then(u => {
      api.getBalances(u.id).then(setBalances).catch(() => {})
      api.getPositions(u.id).then(setPositions).catch(() => {})
      api.getTrades(u.id).then(t => setTrades(t.slice(0, 5))).catch(() => {})
    }).catch(() => {})
  }, [])

  const totalPnl = positions.reduce((s, p) => s + p.unrealizedPnl, 0)

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline', marginBottom: '1.5rem' }}>
        <h1>Dashboard</h1>
        <span className="muted" style={{ fontSize: '0.85rem' }}>Welcome, {session?.user?.email}</span>
      </div>

      {/* Stats row */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '1rem', marginBottom: '1.5rem' }}>
        <div className="card">
          <div className="muted" style={{ fontSize: '0.75rem', textTransform: 'uppercase', letterSpacing: '0.05em', marginBottom: '0.25rem' }}>Total Balances</div>
          <div className="mono" style={{ fontSize: '1.25rem', fontWeight: 600 }}>{balances.length}</div>
        </div>
        <div className="card">
          <div className="muted" style={{ fontSize: '0.75rem', textTransform: 'uppercase', letterSpacing: '0.05em', marginBottom: '0.25rem' }}>Open Positions</div>
          <div className="mono" style={{ fontSize: '1.25rem', fontWeight: 600 }}>{positions.length}</div>
        </div>
        <div className="card">
          <div className="muted" style={{ fontSize: '0.75rem', textTransform: 'uppercase', letterSpacing: '0.05em', marginBottom: '0.25rem' }}>Unrealized PnL</div>
          <div className={`mono ${totalPnl >= 0 ? 'green' : 'red'}`} style={{ fontSize: '1.25rem', fontWeight: 600 }}>${totalPnl.toFixed(2)}</div>
        </div>
      </div>

      {/* Ticker grid with sparklines */}
      <div className="card" style={{ marginBottom: '1.5rem' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline', marginBottom: '0.75rem' }}>
          <h2 style={{ margin: 0 }}>Live Prices</h2>
          <span className="muted" style={{ fontSize: '0.75rem' }}>{Object.keys(prices).length} / {pairs.length} connected</span>
        </div>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: '0.75rem' }}>
          {pairs.map(pair => {
            const p = prices[pair]
            const hist = history[pair] ?? []
            const prevPrice = hist.length >= 2 ? hist[hist.length - 2] : null
            const isUp = prevPrice !== null && p ? p.lastPrice >= prevPrice : true
            return (
              <div key={pair} style={{ background: 'var(--bg)', border: '1px solid var(--border)', borderRadius: 'var(--radius-sm)', padding: '0.75rem 1rem', display: 'flex', flexDirection: 'column', gap: '0.4rem' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <span style={{ fontWeight: 600, fontSize: '0.9rem' }}>{pair.replace('USDT', '/USDT')}</span>
                  {p ? (
                    <span className={`mono ${isUp ? 'green' : 'red'}`} style={{ fontWeight: 600 }}>
                      ${p.lastPrice.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                    </span>
                  ) : (
                    <span className="muted" style={{ fontSize: '0.8rem' }}>--</span>
                  )}
                </div>
                <Sparkline data={hist} color={isUp ? 'var(--green)' : 'var(--red)'} height={36} />
                {p && (
                  <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.7rem' }}>
                    <span className="muted">Bid <span className="green">{p.bid.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</span></span>
                    <span className="muted">Ask <span className="red">{p.ask.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</span></span>
                    <span className="muted">Vol {p.volume.toLocaleString(undefined, { maximumFractionDigits: 0 })}</span>
                  </div>
                )}
              </div>
            )
          })}
        </div>
      </div>

      {/* Bottom row: Wallet + Positions + Recent Trades */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '1rem' }}>
        <div className="card">
          <h2>Wallet</h2>
          {balances.length === 0 ? <p className="muted" style={{ fontSize: '0.85rem' }}>No balances — <Link to="/wallet">deposit funds</Link></p> : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem' }}>
              {balances.map(b => (
                <div key={b.currency} style={{ display: 'flex', justifyContent: 'space-between', padding: '0.25rem 0', borderBottom: '1px solid var(--border)' }}>
                  <span style={{ fontWeight: 600, fontSize: '0.85rem' }}>{b.currency}</span>
                  <span className="mono" style={{ fontSize: '0.85rem' }}>{b.balance.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 8 })}</span>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="card">
          <h2>Positions</h2>
          {positions.length === 0 ? <p className="muted" style={{ fontSize: '0.85rem' }}>No open positions — <Link to="/trade">place a trade</Link></p> : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem' }}>
              {positions.map(p => (
                <div key={p.pair} style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '0.25rem 0', borderBottom: '1px solid var(--border)' }}>
                  <span style={{ fontWeight: 600, fontSize: '0.85rem' }}>{p.pair.replace('USDT', '')}</span>
                  <span className="mono" style={{ fontSize: '0.85rem' }}>{p.netQuantity}</span>
                  <span className={`mono ${p.unrealizedPnl >= 0 ? 'green' : 'red'}`} style={{ fontSize: '0.85rem' }}>${p.unrealizedPnl.toFixed(2)}</span>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="card">
          <h2>Recent Trades</h2>
          {trades.length === 0 ? <p className="muted" style={{ fontSize: '0.85rem' }}>No trades yet</p> : (
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem' }}>
              {trades.map(t => (
                <div key={t.id} style={{ display: 'flex', justifyContent: 'space-between', padding: '0.25rem 0', borderBottom: '1px solid var(--border)', fontSize: '0.8rem' }}>
                  <span className={t.side === 0 ? 'green' : 'red'}>{OrderSideLabel[t.side]}</span>
                  <span>{t.pair.replace('USDT', '')}</span>
                  <span className="mono">{t.quantity} @ ${t.price.toFixed(2)}</span>
                </div>
              ))}
              <Link to="/history" style={{ fontSize: '0.8rem', marginTop: '0.25rem' }}>View all trades &rarr;</Link>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
