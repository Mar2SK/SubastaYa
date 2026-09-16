namespace SubastaYa.Api.Models;

public class Sale
{
    public int Id { get; set; }

    public int AuctionId { get; set; }

    public int BuyerId { get; set; }

    public int SellerId { get; set; }

    public decimal Amount { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}