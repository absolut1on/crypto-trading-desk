import { supabase } from '../supabase'
import type { Order, Trade, WalletBalance, CryptoPair } from '../types'

const API_BASE = (import.meta.env.VITE_GATEWAY_URL || 'http://localhost:8001') + '/api'

async function apiFetch<T>(path: string, options?: RequestInit): Promise<T> {
  const { data: { session } } = await supabase.auth.getSession()
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(session?.access_token && { Authorization: `Bearer ${session.access_token}` }),
    ...options?.headers,
  }

  const res = await fetch(`${API_BASE}${path}`, { ...options, headers })
  if (!res.ok) {
    const body = await res.text().catch(() => '')
    throw new Error(body || `API error: ${res.status}`)
  }
  return res.json()
}

export const api = {
  get: <T>(path: string) => apiFetch<T>(path),
  post: <T>(path: string, body: unknown) => apiFetch<T>(path, { method: 'POST', body: JSON.stringify(body) }),

  getMe: () => apiFetch<{ id: string; email: string; status: number }>('/user/me'),
  getPairs: () => apiFetch<CryptoPair[]>('/cryptopair'),
  getPrices: () => apiFetch<{ symbol: string; bid: string; ask: string; last: string; volume: string; timestamp: number }[]>('/price'),
  getPrice: (pair: string) => apiFetch<Record<string, string>>(`/price/${pair}`),

  getOrders: (userId: string, page = 1) => apiFetch<Order[]>(`/order?userId=${userId}&page=${page}`),
  placeOrder: (order: { userId: string; pair: string; side: number; type: number; quantity: number; price: number | null }) =>
    apiFetch<Order>('/order', { method: 'POST', body: JSON.stringify(order) }),
  cancelOrder: (id: string) => apiFetch<Order>(`/order/${id}/cancel`, { method: 'POST' }),

  getTrades: (userId: string, page = 1) => apiFetch<Trade[]>(`/trade?userId=${userId}&page=${page}`),

  getBalances: (userId: string) => apiFetch<WalletBalance[]>(`/wallet/${userId}/balances`),
  deposit: (userId: string, currency: string, amount: number) =>
    apiFetch<{ message: string }>('/wallet/deposit', { method: 'POST', body: JSON.stringify({ userId, currency, amount }) }),
  withdraw: (userId: string, currency: string, amount: number) =>
    apiFetch<{ message: string }>('/wallet/withdraw', { method: 'POST', body: JSON.stringify({ userId, currency, amount }) }),

  getPositions: (userId: string) => apiFetch<{ pair: string; netQuantity: number; avgEntryPrice: number; marketPrice: number; unrealizedPnl: number; tradeCount: number }[]>(`/position/${userId}`),
}
