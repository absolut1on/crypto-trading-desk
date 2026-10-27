namespace Gateway.API.Entities;

public enum WalletEventType { Deposit, Withdrawal, TradeDebit, TradeCredit, Fee }

public class Wallet
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}

public class WalletEvent
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public WalletEventType EventType { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Wallet Wallet { get; set; } = null!;
}
