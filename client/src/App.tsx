import { Routes, Route, Navigate } from 'react-router-dom'
import { useAuth } from './hooks/useAuth'
import Layout from './components/Layout'
import Login from './pages/Login'
import Dashboard from './pages/Dashboard'

function App() {
  const { session, loading } = useAuth()

  if (loading) return <div style={{ padding: '2rem' }}>Loading...</div>

  return (
    <Routes>
      <Route path="/login" element={session ? <Navigate to="/" /> : <Login />} />
      <Route element={<Layout />}>
        <Route path="/" element={session ? <Dashboard /> : <Navigate to="/login" />} />
      </Route>
    </Routes>
  )
}

export default App
