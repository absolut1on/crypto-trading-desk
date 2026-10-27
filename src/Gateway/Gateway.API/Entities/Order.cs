namespace Gateway.API.Entities;

public enum OrderSide { Buy, Sell }
public enum OrderType { Market, Limit, StopLoss }

public enum OrderStatus
{
    Pending,
    Validated,
    Executing,
    PartiallyFilled,
    Filled,
    Cancelled,
    Expired,
    Failed
}

public class Order
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Pair { get; set; } = string.Empty;
    public OrderSide Side { get; set; }
    public OrderType Type { get; set; }
    public decimal Quantity { get; set; }
    public decimal? Price { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}

public class OrderEvent
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string? Payload { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Order Order { get; set; } = null!;
}
