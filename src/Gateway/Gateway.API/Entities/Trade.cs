namespace Gateway.API.Entities;

public enum TradeStatus { Pending, Settled, Reversed }

public class Trade
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string Pair { get; set; } = string.Empty;
    public OrderSide Side { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal Fee { get; set; }
    public TradeStatus Status { get; set; } = TradeStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Order Order { get; set; } = null!;
    public User User { get; set; } = null!;
}
