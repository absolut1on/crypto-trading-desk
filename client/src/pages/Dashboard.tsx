import { useAuth } from '../hooks/useAuth'
import { usePrices } from '../hooks/usePrices'

export default function Dashboard() {
  const { session } = useAuth()
  const { prices, pairs } = usePrices()

  return (
    <div>
      <h1>Dashboard</h1>
      <p>Welcome, {session?.user?.email}</p>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '1rem', marginTop: '2rem' }}>
        <div style={{ padding: '1.5rem', border: '1px solid #333', borderRadius: 8 }}>
          <h2>Portfolio</h2>
          <p style={{ color: '#888' }}>Positions will appear here</p>
        </div>
        <div style={{ padding: '1.5rem', border: '1px solid #333', borderRadius: 8, gridColumn: 'span 2' }}>
          <h2>Live Prices</h2>
          {pairs.length === 0 ? (
            <p style={{ color: '#888' }}>No pairs configured</p>
          ) : (
            <table style={{ width: '100%', borderCollapse: 'collapse', marginTop: '0.5rem' }}>
              <thead>
                <tr style={{ borderBottom: '1px solid #444', textAlign: 'left' }}>
                  <th style={{ padding: '0.5rem' }}>Pair</th>
                  <th style={{ padding: '0.5rem' }}>Bid</th>
                  <th style={{ padding: '0.5rem' }}>Ask</th>
                  <th style={{ padding: '0.5rem' }}>Last</th>
                  <th style={{ padding: '0.5rem' }}>Volume</th>
                </tr>
              </thead>
              <tbody>
                {pairs.map(pair => {
                  const p = prices[pair]
                  return (
                    <tr key={pair} style={{ borderBottom: '1px solid #333' }}>
                      <td style={{ padding: '0.5rem', fontWeight: 600 }}>{pair.replace('USDT', '/USDT')}</td>
                      {p ? (
                        <>
                          <td style={{ padding: '0.5rem', color: '#4ade80' }}>{p.bid.toFixed(2)}</td>
                          <td style={{ padding: '0.5rem', color: '#f87171' }}>{p.ask.toFixed(2)}</td>
                          <td style={{ padding: '0.5rem' }}>{p.lastPrice.toFixed(2)}</td>
                          <td style={{ padding: '0.5rem', color: '#888' }}>{p.volume.toLocaleString(undefined, { maximumFractionDigits: 0 })}</td>
                        </>
                      ) : (
                        <td colSpan={4} style={{ padding: '0.5rem', color: '#666' }}>Connecting...</td>
                      )}
                    </tr>
                  )
                })}
              </tbody>
            </table>
          )}
        </div>
        <div style={{ padding: '1.5rem', border: '1px solid #333', borderRadius: 8 }}>
          <h2>Recent Trades</h2>
          <p style={{ color: '#888' }}>Trade history will appear here</p>
        </div>
        <div style={{ padding: '1.5rem', border: '1px solid #333', borderRadius: 8 }}>
          <h2>Wallet</h2>
          <p style={{ color: '#888' }}>Balances will appear here</p>
        </div>
      </div>
    </div>
  )
}
