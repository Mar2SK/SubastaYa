namespace SubastaYa.Api.Dtos.Wallets;

public class WalletTransactionItemDto
{
    public int Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int? AuctionId { get; set; }
}