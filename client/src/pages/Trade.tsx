import { useEffect, useState } from 'react'
import { usePrices } from '../hooks/usePrices'
import { api } from '../services/api'
import { OrderSideLabel, OrderStatusLabel, type Order } from '../types'
import Sparkline from '../components/Sparkline'

export default function Trade() {
  const { prices, history, pairs } = usePrices()
  const [userId, setUserId] = useState('')
  const [pair, setPair] = useState('BTCUSDT')
  const [side, setSide] = useState(0)
  const [orderType, setOrderType] = useState(0)
  const [quantity, setQuantity] = useState('')
  const [limitPrice, setLimitPrice] = useState('')
  const [orders, setOrders] = useState<Order[]>([])
  const [submitting, setSubmitting] = useState(false)
  const [msg, setMsg] = useState('')

  useEffect(() => {
    api.getMe().then(u => {
      setUserId(u.id)
      api.getOrders(u.id).then(setOrders).catch(() => {})
    }).catch(() => {})
  }, [])

  const livePrice = prices[pair]
  const hist = history[pair] ?? []

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!userId || !quantity) return
    setSubmitting(true)
    setMsg('')
    try {
      const order = await api.placeOrder({
        userId,
        pair,
        side,
        type: orderType,
        quantity: parseFloat(quantity),
        price: orderType === 1 ? parseFloat(limitPrice) : null,
      })
      setMsg(`${OrderSideLabel[order.side]} ${order.quantity} ${pair} @ $${order.price?.toFixed(2)} — ${OrderStatusLabel[order.status]}`)
      setQuantity('')
      setLimitPrice('')
      api.getOrders(userId).then(setOrders)
    } catch (err: any) {
      setMsg(err.message)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div>
      <h1 style={{ marginBottom: '1.25rem' }}>Trade</h1>

      {/* Chart for selected pair */}
      <div className="card" style={{ marginBottom: '1rem', padding: '1rem' }}>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
          <span style={{ fontWeight: 600 }}>{pair.replace('USDT', '/USDT')}</span>
          {livePrice && <span className="mono" style={{ fontSize: '1.1rem', fontWeight: 600 }}>${livePrice.lastPrice.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</span>}
        </div>
        <Sparkline data={hist} color="var(--accent)" height={80} />
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1.5fr', gap: '1rem' }}>
        <div className="card">
          <h2>Place Order</h2>
          <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '0.65rem' }}>
            <div>
              <label>Pair</label>
              <select value={pair} onChange={e => setPair(e.target.value)}>
                {pairs.map(p => <option key={p} value={p}>{p.replace('USDT', '/USDT')}</option>)}
              </select>
            </div>

            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button type="button" onClick={() => setSide(0)} style={{ flex: 1, background: side === 0 ? 'var(--green)' : 'var(--input-bg)', border: side === 0 ? 'none' : '1px solid var(--border)' }}>Buy</button>
              <button type="button" onClick={() => setSide(1)} style={{ flex: 1, background: side === 1 ? 'var(--red)' : 'var(--input-bg)', border: side === 1 ? 'none' : '1px solid var(--border)' }}>Sell</button>
            </div>

            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button type="button" onClick={() => setOrderType(0)} style={{ flex: 1, background: orderType === 0 ? 'var(--surface-hover)' : 'transparent', border: '1px solid var(--border)' }}>Market</button>
              <button type="button" onClick={() => setOrderType(1)} style={{ flex: 1, background: orderType === 1 ? 'var(--surface-hover)' : 'transparent', border: '1px solid var(--border)' }}>Limit</button>
            </div>

            <div>
              <label>Quantity</label>
              <input type="number" step="any" min="0" value={quantity} onChange={e => setQuantity(e.target.value)} required placeholder="0.00" />
            </div>

            {orderType === 1 && (
              <div>
                <label>Limit Price</label>
                <input type="number" step="any" min="0" value={limitPrice} onChange={e => setLimitPrice(e.target.value)} required placeholder="0.00" />
              </div>
            )}

            {livePrice && (
              <div className="muted" style={{ fontSize: '0.75rem', display: 'flex', gap: '0.75rem' }}>
                <span>Bid <span className="green">${livePrice.bid.toFixed(2)}</span></span>
                <span>Ask <span className="red">${livePrice.ask.toFixed(2)}</span></span>
              </div>
            )}

            <button type="submit" disabled={submitting} style={{ padding: '0.6rem', background: side === 0 ? 'var(--green)' : 'var(--red)', fontWeight: 600, marginTop: '0.25rem' }}>
              {submitting ? 'Placing...' : `${side === 0 ? 'Buy' : 'Sell'} ${pair.replace('USDT', '')}`}
            </button>

            {msg && <div style={{ padding: '0.5rem', background: 'var(--bg)', borderRadius: 'var(--radius-sm)', fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{msg}</div>}
          </form>
        </div>

        <div className="card">
          <h2>Recent Orders</h2>
          {orders.length === 0 ? <p className="muted" style={{ fontSize: '0.85rem' }}>No orders yet</p> : (
            <table>
              <thead>
                <tr>
                  <th>Pair</th>
                  <th>Side</th>
                  <th style={{ textAlign: 'right' }}>Qty</th>
                  <th style={{ textAlign: 'right' }}>Price</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {orders.map(o => (
                  <tr key={o.id}>
                    <td style={{ fontWeight: 600 }}>{o.pair}</td>
                    <td className={o.side === 0 ? 'green' : 'red'}>{OrderSideLabel[o.side]}</td>
                    <td className="mono" style={{ textAlign: 'right' }}>{o.quantity}</td>
                    <td className="mono" style={{ textAlign: 'right' }}>${o.price?.toFixed(2) ?? '—'}</td>
                    <td>{OrderStatusLabel[o.status]}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>
    </div>
  )
}
