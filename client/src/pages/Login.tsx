import { useState } from 'react'
import { useAuth } from '../hooks/useAuth'

export default function Login() {
  const { signIn, signUp } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [isSignUp, setIsSignUp] = useState(false)
  const [error, setError] = useState('')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    const { error } = isSignUp
      ? await signUp(email, password)
      : await signIn(email, password)
    if (error) setError(error.message)
  }

  return (
    <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
      <div className="card" style={{ width: '100%', maxWidth: 400, padding: '2rem' }}>
        <h1 style={{ textAlign: 'center', marginBottom: '0.25rem' }}>Crypto Desk</h1>
        <p className="muted" style={{ textAlign: 'center', marginBottom: '1.5rem', fontSize: '0.85rem' }}>
          {isSignUp ? 'Create your account' : 'Sign in to your account'}
        </p>
        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          <div>
            <label>Email</label>
            <input type="email" value={email} onChange={e => setEmail(e.target.value)} required placeholder="you@example.com" />
          </div>
          <div>
            <label>Password</label>
            <input type="password" value={password} onChange={e => setPassword(e.target.value)} required placeholder="Password" />
          </div>
          {error && <p className="red" style={{ fontSize: '0.85rem' }}>{error}</p>}
          <button type="submit" style={{ padding: '0.6rem', background: 'var(--accent)', fontWeight: 600, marginTop: '0.25rem' }}>
            {isSignUp ? 'Sign Up' : 'Sign In'}
          </button>
        </form>
        <p style={{ marginTop: '1rem', textAlign: 'center' }}>
          <button onClick={() => setIsSignUp(!isSignUp)} style={{ background: 'none', color: 'var(--accent-hover)', fontSize: '0.8rem', padding: 0 }}>
            {isSignUp ? 'Already have an account? Sign in' : "Don't have an account? Sign up"}
          </button>
        </p>
      </div>
    </div>
  )
}
