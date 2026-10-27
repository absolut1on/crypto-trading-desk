import { useEffect, useState } from 'react'
import { api } from '../services/api'
import type { Position } from '../types'

export default function Portfolio() {
  const [positions, setPositions] = useState<Position[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    api.getMe().then(u => api.getPositions(u.id)).then(setPositions).finally(() => setLoading(false))
  }, [])

  const totalPnl = positions.reduce((s, p) => s + p.unrealizedPnl, 0)

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'baseline', marginBottom: '1.25rem' }}>
        <h1>Portfolio</h1>
        {positions.length > 0 && (
          <span style={{ fontSize: '1rem' }}>
            Total PnL: <span className={`mono ${totalPnl >= 0 ? 'green' : 'red'}`} style={{ fontWeight: 600 }}>${totalPnl.toFixed(2)}</span>
          </span>
        )}
      </div>
      <div className="card">
        {loading ? <p className="muted">Loading...</p> : positions.length === 0 ? <p className="muted" style={{ fontSize: '0.85rem' }}>No open positions</p> : (
          <table>
            <thead>
              <tr>
                <th>Pair</th>
                <th style={{ textAlign: 'right' }}>Quantity</th>
                <th style={{ textAlign: 'right' }}>Avg Entry</th>
                <th style={{ textAlign: 'right' }}>Market Price</th>
                <th style={{ textAlign: 'right' }}>Unrealized PnL</th>
                <th style={{ textAlign: 'right' }}>Trades</th>
              </tr>
            </thead>
            <tbody>
              {positions.map(p => (
                <tr key={p.pair}>
                  <td style={{ fontWeight: 600 }}>{p.pair.replace('USDT', '/USDT')}</td>
                  <td className="mono" style={{ textAlign: 'right' }}>{p.netQuantity}</td>
                  <td className="mono" style={{ textAlign: 'right' }}>${p.avgEntryPrice.toFixed(2)}</td>
                  <td className="mono" style={{ textAlign: 'right' }}>${p.marketPrice.toFixed(2)}</td>
                  <td className={`mono ${p.unrealizedPnl >= 0 ? 'green' : 'red'}`} style={{ textAlign: 'right' }}>${p.unrealizedPnl.toFixed(2)}</td>
                  <td style={{ textAlign: 'right' }}>{p.tradeCount}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  )
}
