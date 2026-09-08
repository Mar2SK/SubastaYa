namespace SubastaYa.Api.Models;

public class Auction
{
    public int Id { get; set; }

    public int SellerId { get; set; }

    public int CategoryId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public decimal BasePrice { get; set; }

    public decimal MinimumIncrement { get; set; }

    public DateTime StartAtUtc { get; set; }

    public DateTime EndAtUtc { get; set; }

    public string Status { get; set; } = "PROGRAMADA";

    public int Version { get; set; } = 1;

    public User Seller { get; set; } = null!;

    public Category Category { get; set; } = null!;

    public List<Bid> Bids { get; set; } = [];

    public List<TransactionLedger> Transactions { get; set; } = [];
}