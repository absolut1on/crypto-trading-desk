import { useAuth } from '../hooks/useAuth'

export default function Dashboard() {
  const { session } = useAuth()

  return (
    <div>
      <h1>Dashboard</h1>
      <p>Welcome, {session?.user?.email}</p>
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(300px, 1fr))', gap: '1rem', marginTop: '2rem' }}>
        <div style={{ padding: '1.5rem', border: '1px solid #333', borderRadius: 8 }}>
          <h2>Portfolio</h2>
          <p style={{ color: '#888' }}>Positions will appear here</p>
        </div>
        <div style={{ padding: '1.5rem', border: '1px solid #333', borderRadius: 8 }}>
          <h2>Live Prices</h2>
          <p style={{ color: '#888' }}>Real-time ticker will appear here</p>
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
