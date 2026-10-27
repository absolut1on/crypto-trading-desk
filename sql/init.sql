-- Materialized views (managed outside EF Core)
-- Run after EF Core migrations have created the base tables

-- Portfolio positions: net quantity and avg entry price per user per pair
CREATE MATERIALIZED VIEW IF NOT EXISTS portfolio_positions AS
SELECT
    t."UserId" AS user_id,
    t."Pair" AS pair,
    SUM(CASE WHEN t."Side" = 'Buy' THEN t."Quantity" ELSE -t."Quantity" END) AS net_quantity,
    SUM(CASE WHEN t."Side" = 'Buy' THEN t."Quantity" * t."Price" ELSE 0 END) /
        NULLIF(SUM(CASE WHEN t."Side" = 'Buy' THEN t."Quantity" ELSE 0 END), 0) AS avg_entry_price,
    COUNT(*) AS trade_count,
    MAX(t."CreatedAt") AS last_trade_at
FROM "Trades" t
WHERE t."Status" = 'Settled'
GROUP BY t."UserId", t."Pair";

CREATE UNIQUE INDEX IF NOT EXISTS idx_portfolio_positions_user_pair ON portfolio_positions (user_id, pair);

-- Wallet balances: latest balance per user per currency
CREATE MATERIALIZED VIEW IF NOT EXISTS wallet_balances AS
SELECT DISTINCT ON (w."UserId", w."Currency")
    w."UserId" AS user_id,
    w."Currency" AS currency,
    we."BalanceAfter" AS balance,
    we."Timestamp" AS last_updated
FROM "WalletEvents" we
JOIN "Wallets" w ON w."Id" = we."WalletId"
ORDER BY w."UserId", w."Currency", we."Timestamp" DESC;

CREATE UNIQUE INDEX IF NOT EXISTS idx_wallet_balances_user_currency ON wallet_balances (user_id, currency);

-- Daily P&L: profit/loss per user per pair per day
CREATE MATERIALIZED VIEW IF NOT EXISTS daily_pnl AS
SELECT
    t."UserId" AS user_id,
    t."Pair" AS pair,
    DATE(t."CreatedAt") AS trade_date,
    SUM(CASE WHEN t."Side" = 'Sell' THEN t."Quantity" * t."Price" ELSE -t."Quantity" * t."Price" END) - SUM(t."Fee") AS realized_pnl,
    COUNT(*) AS trade_count
FROM "Trades" t
WHERE t."Status" = 'Settled'
GROUP BY t."UserId", t."Pair", DATE(t."CreatedAt");

CREATE UNIQUE INDEX IF NOT EXISTS idx_daily_pnl_user_pair_date ON daily_pnl (user_id, pair, trade_date);

-- Pair statistics: 24h trading stats per pair
CREATE MATERIALIZED VIEW IF NOT EXISTS pair_statistics AS
SELECT
    t."Pair" AS pair,
    COUNT(*) AS trade_count_24h,
    SUM(t."Quantity") AS volume_24h,
    SUM(t."Quantity" * t."Price") AS notional_24h
FROM "Trades" t
WHERE t."Status" = 'Settled' AND t."CreatedAt" > NOW() - INTERVAL '24 hours'
GROUP BY t."Pair";

CREATE UNIQUE INDEX IF NOT EXISTS idx_pair_statistics_pair ON pair_statistics (pair);
