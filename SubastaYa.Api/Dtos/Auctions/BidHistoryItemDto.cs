namespace SubastaYa.Api.Dtos.Auctions;

public class BidHistoryItemDto
{
    public int Id { get; set; }

    public string BuyerAlias { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime BidAtUtc { get; set; }
}