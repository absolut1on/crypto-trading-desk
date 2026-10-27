using Microsoft.AspNetCore.SignalR;

namespace Gateway.API.Hubs;

public class PriceHub : Hub
{
    public async Task SubscribeToPair(string pair)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, pair);
    }

    public async Task UnsubscribeFromPair(string pair)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, pair);
    }
}
