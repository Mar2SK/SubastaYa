namespace SubastaYa.Api.Models;

public class Bid
{
    public int Id { get; set; }

    public int AuctionId { get; set; }

    public int BuyerId { get; set; }

    public decimal Amount { get; set; }

    public DateTime BidAtUtc { get; set; } = DateTime.UtcNow;

    public Auction Auction { get; set; } = null!;

    public User Buyer { get; set; } = null!;
}