namespace SubastaYa.Api.Models;

public class TransactionLedger
{
    public int Id { get; set; }

    public int WalletId { get; set; }

    public string Type { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public int? AuctionId { get; set; }

    public Wallet Wallet { get; set; } = null!;

    public Auction? Auction { get; set; }
}