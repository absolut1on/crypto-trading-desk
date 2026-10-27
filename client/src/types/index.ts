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
  side: number
  type: number
  quantity: number
  price: number | null
  status: number
  createdAt: string
}

export interface Trade {
  id: string
  orderId: string
  userId: string
  pair: string
  side: number
  quantity: number
  price: number
  fee: number
  status: number
  createdAt: string
}

export interface WalletBalance {
  currency: string
  balance: number
}

export interface Position {
  pair: string
  netQuantity: number
  avgEntryPrice: number
  marketPrice: number
  unrealizedPnl: number
  tradeCount: number
}

export interface PriceTicker {
  symbol: string
  bid: number
  ask: number
  lastPrice: number
  volume: number
  timestamp: string
}

export const OrderSideLabel = ['Buy', 'Sell'] as const
export const OrderTypeLabel = ['Market', 'Limit', 'StopLoss'] as const
export const OrderStatusLabel = ['Pending', 'Validated', 'Executing', 'PartiallyFilled', 'Filled', 'Cancelled', 'Failed', 'Expired'] as const
export const TradeStatusLabel = ['Pending', 'Settled', 'Reversed'] as const
