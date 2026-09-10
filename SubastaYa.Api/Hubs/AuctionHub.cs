using Microsoft.AspNetCore.SignalR;

namespace SubastaYa.Api.Hubs;

public class AuctionHub : Hub
{
    public Task JoinAuction(int auctionId)
    {
        return Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"auction-{auctionId}");
    }

    public Task LeaveAuction(int auctionId)
    {
        return Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            $"auction-{auctionId}");
    }
}