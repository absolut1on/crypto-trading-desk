using Gateway.API.Data;
using Gateway.API.Entities;

namespace Gateway.API.Sagas;

public class TradeSaga
{
    private readonly AppDbContext _db;

    public TradeSaga(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Saga> StartAsync(Order order)
    {
        var saga = new Saga
        {
            Id = Guid.NewGuid(),
            SagaType = "PlaceOrder",
            CurrentStep = "ValidateOrder",
            Payload = System.Text.Json.JsonSerializer.Serialize(new { order.Id, order.Pair, order.Side, order.Quantity, order.Price })
        };

        _db.Sagas.Add(saga);
        await _db.SaveChangesAsync();
        return saga;
    }

    // Steps will be implemented in Phase 3:
    // 1. ValidateOrder — pair exists, market open
    // 2. ReserveFunds — check wallet, hold amount
    // 3. ExecuteOrder — match against market price
    // 4. CreateTrade — append trade event
    // 5. UpdatePosition — refresh materialized view
    // 6. SettleWallet — debit/credit wallet
    // Compensating: UnreserveFunds on failure
}
