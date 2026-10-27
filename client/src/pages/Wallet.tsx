import { useEffect, useState } from 'react'
import { api } from '../services/api'
import type { WalletBalance } from '../types'

export default function Wallet() {
  const [userId, setUserId] = useState('')
  const [balances, setBalances] = useState<WalletBalance[]>([])
  const [currency, setCurrency] = useState('USDT')
  const [amount, setAmount] = useState('')
  const [action, setAction] = useState<'deposit' | 'withdraw'>('deposit')
  const [msg, setMsg] = useState('')
  const [loading, setLoading] = useState(false)

  const load = (uid: string) => api.getBalances(uid).then(setBalances)

  useEffect(() => {
    api.getMe().then(u => { setUserId(u.id); load(u.id) })
  }, [])

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!userId || !amount) return
    setLoading(true)
    setMsg('')
    try {
      const fn = action === 'deposit' ? api.deposit : api.withdraw
      const res = await fn(userId, currency, parseFloat(amount))
      setMsg(res.message)
      setAmount('')
      load(userId)
    } catch (err: any) {
      setMsg(err.message)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div>
      <h1 style={{ marginBottom: '1.25rem' }}>Wallet</h1>
      <div style={{ display: 'grid', gridTemplateColumns: '1.2fr 1fr', gap: '1rem' }}>
        <div className="card">
          <h2>Balances</h2>
          {balances.length === 0 ? <p className="muted" style={{ fontSize: '0.85rem' }}>No balances yet</p> : (
            <table>
              <thead>
                <tr>
                  <th>Currency</th>
                  <th style={{ textAlign: 'right' }}>Balance</th>
                </tr>
              </thead>
              <tbody>
                {balances.map(b => (
                  <tr key={b.currency}>
                    <td style={{ fontWeight: 600 }}>{b.currency}</td>
                    <td className="mono" style={{ textAlign: 'right' }}>
                      {b.balance.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 8 })}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        <div className="card">
          <h2>Deposit / Withdraw</h2>
          <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '0.65rem' }}>
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button type="button" onClick={() => setAction('deposit')} style={{ flex: 1, background: action === 'deposit' ? 'var(--green)' : 'var(--input-bg)', border: action === 'deposit' ? 'none' : '1px solid var(--border)' }}>Deposit</button>
              <button type="button" onClick={() => setAction('withdraw')} style={{ flex: 1, background: action === 'withdraw' ? 'var(--red)' : 'var(--input-bg)', border: action === 'withdraw' ? 'none' : '1px solid var(--border)' }}>Withdraw</button>
            </div>

            <div>
              <label>Currency</label>
              <input type="text" value={currency} onChange={e => setCurrency(e.target.value.toUpperCase())} placeholder="USDT" />
            </div>

            <div>
              <label>Amount</label>
              <input type="number" step="any" min="0" value={amount} onChange={e => setAmount(e.target.value)} required placeholder="0.00" />
            </div>

            <button type="submit" disabled={loading} style={{ padding: '0.6rem', background: action === 'deposit' ? 'var(--green)' : 'var(--red)', fontWeight: 600, marginTop: '0.25rem' }}>
              {loading ? 'Processing...' : action === 'deposit' ? 'Deposit' : 'Withdraw'}
            </button>

            {msg && <div style={{ padding: '0.5rem', background: 'var(--bg)', borderRadius: 'var(--radius-sm)', fontSize: '0.8rem', color: 'var(--text-secondary)' }}>{msg}</div>}
          </form>
        </div>
      </div>
    </div>
  )
}
