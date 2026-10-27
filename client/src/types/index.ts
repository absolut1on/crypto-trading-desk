export interface CryptoPair {
  id: string
  symbol: string
  baseCurrency: string
  quoteCurrency: string
  isActive: boolean
}

export interface Order {
  id: string
  userId: string
  pair: string
  side: 'Buy' | 'Sell'
  type: 'Market' | 'Limit' | 'StopLoss'
  quantity: number
  price: number | null
  status: string
  createdAt: string
}

export interface Trade {
  id: string
  orderId: string
  userId: string
  pair: string
  side: 'Buy' | 'Sell'
  quantity: number
  price: number
  fee: number
  status: string
  createdAt: string
}

export interface WalletBalance {
  currency: string
  balance: number
}

export interface PriceTicker {
  symbol: string
  bid: number
  ask: number
  lastPrice: number
  volume: number
  timestamp: string
}
