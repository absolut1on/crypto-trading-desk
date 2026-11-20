# Crypto Trading Desk

A real-time cryptocurrency trading desk that streams live prices from Binance, lets users deposit/withdraw funds, place market and limit orders, and track positions with unrealized PnL. Dark-mode-only UI.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | .NET 9 minimal APIs (Gateway + Rates microservices) |
| Frontend | React 19 + Vite + TypeScript |
| Auth | Supabase Auth (JWT) |
| Database | PostgreSQL 16 |
| Cache & Messaging | Redis 7 (cache + Streams) |
| Real-time | SignalR (WebSockets) |
| Charts | recharts |
| Monitoring | Grafana 11 |
| Infrastructure | Docker Compose (6 containers) |

## Architecture

```
Binance WebSocket
    |
    v
[Rates API] --Redis Stream--> [Gateway API] --SignalR--> [React Client]
                                    |
                               [PostgreSQL]
```

- **Rates API** connects to Binance's WebSocket, caches prices in Redis, and publishes ticks to a Redis Stream
- **Gateway API** consumes the stream, broadcasts to browsers via SignalR, and handles all business logic (orders, wallets, trades, positions)
- **React Client** displays live prices, sparkline charts, and provides the trading interface

## Prerequisites

- [Docker](https://www.docker.com/products/docker-desktop/) installed
- A [Supabase](https://supabase.com/) project (free tier works)

### Supabase Setup

1. Create a project at [supabase.com](https://supabase.com/)
2. Go to **Authentication > Providers** and make sure Email is enabled
3. Go to **Authentication > URL Configuration** and disable "Confirm email" for local development
4. Grab these from **Settings > API**:
   - Project URL
   - `anon` public key
   - JWT Secret (under **Settings > API > JWT Settings**)

## Getting Started

1. **Clone the repo**

```bash
git clone https://github.com/absolut1on/crypto-trading-desk.git
cd crypto-trading-desk
```

2. **Create a `.env` file** in the project root

```env
SUPABASE_JWT_SECRET=your-jwt-secret-here
SUPABASE_URL=https://your-project-ref.supabase.co/auth/v1
VITE_SUPABASE_URL=https://your-project-ref.supabase.co
VITE_SUPABASE_ANON_KEY=your-anon-key-here
VITE_GATEWAY_URL=http://localhost:8001
```

3. **Start everything**

```bash
docker compose up --build
```

4. **Open the app** at [http://localhost:3000](http://localhost:3000) and sign up for an account

## Services

| Service | URL | Description |
|---------|-----|-------------|
| Client | [localhost:3000](http://localhost:3000) | React app |
| Gateway API | [localhost:8001](http://localhost:8001/swagger) | Main API ([Swagger docs](http://localhost:8001/swagger)) |
| Rates API | [localhost:8002](http://localhost:8002/health) | Binance price consumer |
| PostgreSQL | localhost:5432 | Database (user: `cryptodesk`, password: `cryptodesk`) |
| Redis | localhost:6379 | Cache + message stream |
| Grafana | [localhost:9000](http://localhost:9000) | Monitoring (admin/admin) |

## Features

### Live Prices
- Real-time streaming from Binance for 8 pairs: BTC, ETH, BNB, SOL, XRP, ADA, DOGE, DOT (all vs USDT)
- Sparkline charts showing last 60 price ticks per pair
- Bid/Ask/Volume display

### Trading
- Market orders (executes at current market price)
- Limit orders (executes at your specified price)
- Buy and sell for any active pair
- 0.1% trading fee
- Order state machine: Pending > Validated > Executing > Filled

### Wallet
- Deposit and withdraw any currency
- Event-sourced balance ledger (full audit trail)
- Balance checks before order execution

### Portfolio
- Aggregated positions across all trades
- Average entry price calculation
- Live unrealized PnL (using real-time market prices)

### Trade History
- Paginated trade log
- Date, pair, side, quantity, price, fee, total, status

### Auth
- Email/password sign-up and sign-in via Supabase
- JWT-protected API endpoints
- Auto-provisioned user records on first login

## Development

After making code changes, rebuild the affected container:

```bash
# Frontend changes
docker compose up -d --build client

# Gateway API changes
docker compose up -d --build gateway-api

# Rates API changes
docker compose up -d --build rates-api
```

### Useful Commands

```bash
# View logs for a service
docker compose logs -f gateway-api

# Open a psql shell
docker compose exec postgres psql -U cryptodesk -d cryptodesk

# Stop everything
docker compose down

# Stop and wipe the database (fresh start)
docker compose down -v
```

### Running the Client Locally (with hot reload)

If you want live reload during frontend development:

```bash
cd client
npm install
npm run dev
```

This runs Vite's dev server on `localhost:5173`. Keep the backend containers running via Docker.

## API Endpoints

All endpoints require a Supabase JWT Bearer token except `/health`.

| Method | Path | Description |
|--------|------|-------------|
| GET | `/health` | Health check |
| GET | `/api/user/me` | Current user (auto-creates on first call) |
| GET | `/api/cryptopair` | List active trading pairs |
| GET | `/api/price` | All live prices |
| GET | `/api/price/{pair}` | Single pair price |
| POST | `/api/order` | Place an order |
| GET | `/api/order?userId={id}` | List orders |
| POST | `/api/order/{id}/cancel` | Cancel an order |
| GET | `/api/trade?userId={id}` | List trades |
| GET | `/api/wallet/{userId}/balances` | Get wallet balances |
| POST | `/api/wallet/deposit` | Deposit funds |
| POST | `/api/wallet/withdraw` | Withdraw funds |
| GET | `/api/position/{userId}` | Get positions with live PnL |

## Project Structure

```
crypto-trading-desk/
├── docker-compose.yml
├── .env
├── src/
│   ├── Gateway/Gateway.API/     # Main API
│   │   ├── Auth/                # Supabase JWT validation
│   │   ├── Controllers/         # REST endpoints
│   │   ├── Data/                # EF Core DbContext + migrations
│   │   ├── Entities/            # Domain models + enums
│   │   ├── Hubs/                # SignalR price hub
│   │   ├── Sagas/               # Trade execution orchestrator
│   │   ├── Services/            # Redis stream, price consumer
│   │   └── StateMachines/       # Order + User state transitions
│   └── Rates/Rates.API/         # Binance WebSocket consumer
│       └── Services/
└── client/                      # React frontend
    └── src/
        ├── components/          # Layout, Sparkline
        ├── hooks/               # useAuth, usePrices
        ├── pages/               # Dashboard, Trade, Wallet, Portfolio, History, Login
        ├── services/            # API client
        └── types/               # TypeScript interfaces
```
