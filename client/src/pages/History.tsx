import { useEffect, useState } from 'react'
import { api } from '../services/api'
import { OrderSideLabel, TradeStatusLabel, type Trade } from '../types'

export default function History() {
  const [trades, setTrades] = useState<Trade[]>([])
  const [loading, setLoading] = useState(true)
  const [page, setPage] = useState(1)

  useEffect(() => {
    api.getMe().then(u => api.getTrades(u.id, page)).then(setTrades).finally(() => setLoading(false))
  }, [page])

  return (
    <div>
      <h1 style={{ marginBottom: '1.25rem' }}>Trade History</h1>
      <div className="card">
        {loading ? <p className="muted">Loading...</p> : trades.length === 0 ? <p className="muted" style={{ fontSize: '0.85rem' }}>No trades yet</p> : (
          <>
            <table>
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Pair</th>
                  <th>Side</th>
                  <th style={{ textAlign: 'right' }}>Quantity</th>
                  <th style={{ textAlign: 'right' }}>Price</th>
                  <th style={{ textAlign: 'right' }}>Fee</th>
                  <th style={{ textAlign: 'right' }}>Total</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {trades.map(t => {
                  const total = t.quantity * t.price
                  return (
                    <tr key={t.id}>
                      <td className="muted" style={{ fontSize: '0.8rem' }}>{new Date(t.createdAt).toLocaleString()}</td>
                      <td style={{ fontWeight: 600 }}>{t.pair}</td>
                      <td className={t.side === 0 ? 'green' : 'red'}>{OrderSideLabel[t.side]}</td>
                      <td className="mono" style={{ textAlign: 'right' }}>{t.quantity}</td>
                      <td className="mono" style={{ textAlign: 'right' }}>${t.price.toFixed(2)}</td>
                      <td className="mono muted" style={{ textAlign: 'right' }}>${t.fee.toFixed(2)}</td>
                      <td className="mono" style={{ textAlign: 'right' }}>${total.toFixed(2)}</td>
                      <td>{TradeStatusLabel[t.status]}</td>
                    </tr>
                  )
                })}
              </tbody>
            </table>
            <div style={{ display: 'flex', gap: '0.5rem', marginTop: '1rem', alignItems: 'center' }}>
              <button disabled={page <= 1} onClick={() => setPage(p => p - 1)} style={{ background: 'var(--input-bg)', border: '1px solid var(--border)' }}>Prev</button>
              <span className="muted" style={{ fontSize: '0.8rem' }}>Page {page}</span>
              <button disabled={trades.length < 20} onClick={() => setPage(p => p + 1)} style={{ background: 'var(--input-bg)', border: '1px solid var(--border)' }}>Next</button>
            </div>
          </>
        )}
      </div>
    </div>
  )
}
