using System.Text.Json;
using Gateway.API.Data;
using Gateway.API.Entities;
using Gateway.API.StateMachines;
using Gateway.API.Services;
using Microsoft.EntityFrameworkCore;

namespace Gateway.API.Sagas;

public class TradeSaga
{
    private readonly AppDbContext _db;
    private readonly RedisStreamService _redis;
    private const decimal FeeRate = 0.001m;

    public TradeSaga(AppDbContext db, RedisStreamService redis)
    {
        _db = db;
        _redis = redis;
    }

    public async Task<(Order Order, string? Error)> ExecuteAsync(Order order)
    {
        var saga = new Saga
        {
            Id = Guid.NewGuid(),
            SagaType = "PlaceOrder",
            CurrentStep = "ValidateOrder",
            Payload = JsonSerializer.Serialize(new { order.Id, order.Pair, order.Side, order.Quantity, order.Price })
        };
        _db.Sagas.Add(saga);
        await _db.SaveChangesAsync();

        try
        {
            await RunStep(saga, "ValidateOrder", () => ValidateOrder(order));
            await RunStep(saga, "ReserveFunds", () => ReserveFunds(order));
            await RunStep(saga, "ExecuteOrder", () => ExecuteOrder(order));
            await RunStep(saga, "CreateTrade", () => CreateTrade(order));
            await RunStep(saga, "SettleWallet", () => SettleWallet(order));

            saga.State = SagaState.Completed;
            saga.CurrentStep = "Done";
            saga.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return (order, null);
        }
        catch (SagaException ex)
        {
            saga.State = SagaState.Failed;
            saga.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            if (OrderStateMachine.CanTransition(order.Status, "Fail"))
            {
                order.Status = OrderStateMachine.Transition(order.Status, "Fail");
                order.UpdatedAt = DateTime.UtcNow;
                AppendOrderEvent(order, "Fail", ex.Message);
                await _db.SaveChangesAsync();
            }

            return (order, ex.Message);
        }
    }

    private async Task ValidateOrder(Order order)
    {
        var pair = await _db.CryptoPairs.FirstOrDefaultAsync(p => p.Symbol == order.Pair);
        if (pair is null || !pair.IsActive)
            throw new SagaException($"Pair {order.Pair} not found or inactive");

        if (order.Quantity <= 0)
            throw new SagaException("Quantity must be positive");

        order.Status = OrderStateMachine.Transition(order.Status, "Validate");
        order.UpdatedAt = DateTime.UtcNow;
        AppendOrderEvent(order, "Validate", null);
    }

    private async Task ReserveFunds(Order order)
    {
        var price = await GetMarketPrice(order);
        if (order.Side == OrderSide.Buy)
        {
            var cost = order.Quantity * price;
            var wallet = await GetOrCreateWallet(order.UserId, "USDT");
            var balance = await GetBalance(wallet.Id);
            if (balance < cost)
                throw new SagaException($"Insufficient USDT: need {cost}, have {balance}");
        }
        else
        {
            var baseCurrency = order.Pair.Replace("USDT", "");
            var wallet = await GetOrCreateWallet(order.UserId, baseCurrency);
            var balance = await GetBalance(wallet.Id);
            if (balance < order.Quantity)
                throw new SagaException($"Insufficient {baseCurrency}: need {order.Quantity}, have {balance}");
        }
    }

    private async Task ExecuteOrder(Order order)
    {
        var price = await GetMarketPrice(order);
        order.Price = price;
        order.Status = OrderStateMachine.Transition(order.Status, "Execute");
        order.UpdatedAt = DateTime.UtcNow;
        AppendOrderEvent(order, "Execute", $"price={price}");

        order.Status = OrderStateMachine.Transition(order.Status, "Fill");
        order.UpdatedAt = DateTime.UtcNow;
        AppendOrderEvent(order, "Fill", null);
    }

    private async Task CreateTrade(Order order)
    {
        var fee = order.Quantity * order.Price!.Value * FeeRate;
        var trade = new Trade
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            UserId = order.UserId,
            Pair = order.Pair,
            Side = order.Side,
            Quantity = order.Quantity,
            Price = order.Price!.Value,
            Fee = fee,
            Status = TradeStatus.Settled
        };
        _db.Trades.Add(trade);
        await _db.SaveChangesAsync();
    }

    private async Task SettleWallet(Order order)
    {
        var baseCurrency = order.Pair.Replace("USDT", "");
        var notional = order.Quantity * order.Price!.Value;
        var fee = notional * FeeRate;

        if (order.Side == OrderSide.Buy)
        {
            await DebitWallet(order.UserId, "USDT", notional + fee, WalletEventType.TradeDebit);
            await CreditWallet(order.UserId, baseCurrency, order.Quantity, WalletEventType.TradeCredit);
        }
        else
        {
            await DebitWallet(order.UserId, baseCurrency, order.Quantity, WalletEventType.TradeDebit);
            await CreditWallet(order.UserId, "USDT", notional - fee, WalletEventType.TradeCredit);
        }
    }

    private async Task<decimal> GetMarketPrice(Order order)
    {
        if (order.Type == OrderType.Limit && order.Price.HasValue)
            return order.Price.Value;

        var json = await _redis.GetLatestAsync($"price:{order.Pair}");
        if (json is null)
            throw new SagaException($"No market price available for {order.Pair}");

        using var doc = JsonDocument.Parse(json);
        var lastPrice = doc.RootElement.GetProperty("c").GetString();
        return decimal.Parse(lastPrice!);
    }

    private async Task<Wallet> GetOrCreateWallet(Guid userId, string currency)
    {
        await EnsureUserExists(userId);
        var wallet = await _db.Wallets.FirstOrDefaultAsync(w => w.UserId == userId && w.Currency == currency);
        if (wallet is not null) return wallet;

        wallet = new Wallet { Id = Guid.NewGuid(), UserId = userId, Currency = currency };
        _db.Wallets.Add(wallet);
        await _db.SaveChangesAsync();
        return wallet;
    }

    private async Task EnsureUserExists(Guid userId)
    {
        if (await _db.Users.AnyAsync(u => u.Id == userId)) return;
        _db.Users.Add(new User { Id = userId, Email = $"{userId}@supabase", Status = UserStatus.Active });
        await _db.SaveChangesAsync();
    }

    private async Task<decimal> GetBalance(Guid walletId)
    {
        var lastEvent = await _db.WalletEvents
            .Where(e => e.WalletId == walletId)
            .OrderByDescending(e => e.Timestamp)
            .FirstOrDefaultAsync();
        return lastEvent?.BalanceAfter ?? 0m;
    }

    private async Task DebitWallet(Guid userId, string currency, decimal amount, WalletEventType eventType)
    {
        var wallet = await GetOrCreateWallet(userId, currency);
        var balance = await GetBalance(wallet.Id);
        _db.WalletEvents.Add(new WalletEvent
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            EventType = eventType,
            Amount = -amount,
            BalanceAfter = balance - amount
        });
        await _db.SaveChangesAsync();
    }

    private async Task CreditWallet(Guid userId, string currency, decimal amount, WalletEventType eventType)
    {
        var wallet = await GetOrCreateWallet(userId, currency);
        var balance = await GetBalance(wallet.Id);
        _db.WalletEvents.Add(new WalletEvent
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            EventType = eventType,
            Amount = amount,
            BalanceAfter = balance + amount
        });
        await _db.SaveChangesAsync();
    }

    public async Task DepositAsync(Guid userId, string currency, decimal amount)
    {
        var wallet = await GetOrCreateWallet(userId, currency);
        var balance = await GetBalance(wallet.Id);
        _db.WalletEvents.Add(new WalletEvent
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            EventType = WalletEventType.Deposit,
            Amount = amount,
            BalanceAfter = balance + amount
        });
        await _db.SaveChangesAsync();
    }

    public async Task WithdrawAsync(Guid userId, string currency, decimal amount)
    {
        var wallet = await GetOrCreateWallet(userId, currency);
        var balance = await GetBalance(wallet.Id);
        if (balance < amount)
            throw new SagaException($"Insufficient {currency}: need {amount}, have {balance}");

        _db.WalletEvents.Add(new WalletEvent
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            EventType = WalletEventType.Withdrawal,
            Amount = -amount,
            BalanceAfter = balance - amount
        });
        await _db.SaveChangesAsync();
    }

    private void AppendOrderEvent(Order order, string eventType, string? payload)
    {
        _db.OrderEvents.Add(new OrderEvent
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            EventType = eventType,
            Payload = payload
        });
    }

    private async Task RunStep(Saga saga, string stepName, Func<Task> action)
    {
        var step = new SagaStep
        {
            Id = Guid.NewGuid(),
            SagaId = saga.Id,
            StepName = stepName,
            Status = SagaStepStatus.Running
        };
        _db.SagaSteps.Add(step);
        saga.CurrentStep = stepName;
        saga.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        try
        {
            await action();
            step.Status = SagaStepStatus.Completed;
            step.Timestamp = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            step.Status = SagaStepStatus.Failed;
            step.Result = ex.Message;
            step.Timestamp = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            throw;
        }
    }
}

public class SagaException : Exception
{
    public SagaException(string message) : base(message) { }
}
