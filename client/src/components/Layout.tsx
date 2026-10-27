import { Outlet, Link } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

export default function Layout() {
  const { signOut } = useAuth()

  return (
    <div>
      <nav style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '1rem 2rem', borderBottom: '1px solid #333' }}>
        <div style={{ display: 'flex', gap: '1.5rem', alignItems: 'center' }}>
          <Link to="/" style={{ fontWeight: 'bold', fontSize: '1.2rem', textDecoration: 'none' }}>Crypto Desk</Link>
          <Link to="/" style={{ textDecoration: 'none' }}>Dashboard</Link>
        </div>
        <button onClick={signOut} style={{ background: 'none', border: '1px solid #666', padding: '0.4rem 1rem', borderRadius: 4, cursor: 'pointer', color: 'inherit' }}>
          Sign Out
        </button>
      </nav>
      <main style={{ padding: '2rem' }}>
        <Outlet />
      </main>
    </div>
  )
}
