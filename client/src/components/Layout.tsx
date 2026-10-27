import { Outlet, NavLink } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

const links = [
  { to: '/', label: 'Dashboard', end: true },
  { to: '/trade', label: 'Trade' },
  { to: '/wallet', label: 'Wallet' },
  { to: '/portfolio', label: 'Portfolio' },
  { to: '/history', label: 'History' },
]

export default function Layout() {
  const { signOut } = useAuth()

  return (
    <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      <nav style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', padding: '0 1.5rem', height: 56, background: 'var(--surface)', borderBottom: '1px solid var(--border)', position: 'sticky', top: 0, zIndex: 50 }}>
        <div style={{ display: 'flex', gap: '0.25rem', alignItems: 'center' }}>
          <span style={{ fontWeight: 700, fontSize: '1.1rem', marginRight: '1.5rem', color: 'var(--accent-hover)', letterSpacing: '-0.02em' }}>Crypto Desk</span>
          {links.map(l => (
            <NavLink
              key={l.to}
              to={l.to}
              end={l.end}
              style={({ isActive }) => ({
                padding: '0.4rem 0.75rem',
                borderRadius: 6,
                fontSize: '0.875rem',
                fontWeight: isActive ? 600 : 400,
                color: isActive ? '#fff' : 'var(--text-secondary)',
                background: isActive ? 'rgba(99, 102, 241, 0.15)' : 'transparent',
                textDecoration: 'none',
                transition: 'all 0.15s',
              })}
            >
              {l.label}
            </NavLink>
          ))}
        </div>
        <button
          onClick={signOut}
          style={{ background: 'transparent', border: '1px solid var(--border)', padding: '0.35rem 0.85rem', color: 'var(--text-secondary)', fontSize: '0.8rem' }}
        >
          Sign Out
        </button>
      </nav>
      <main style={{ flex: 1, padding: '1.5rem 2rem', maxWidth: 1400, width: '100%', margin: '0 auto' }}>
        <Outlet />
      </main>
    </div>
  )
}
