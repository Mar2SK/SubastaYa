namespace SubastaYa.Api.Dtos.Bids;

public class BidResultDto
{
    public int BidId { get; set; }

    public int AuctionId { get; set; }

    public int BuyerId { get; set; }

    public decimal Amount { get; set; }

    public DateTime BidAtUtc { get; set; }

    public DateTime EndAtUtc { get; set; }

    public bool AntiSnipingApplied { get; set; }
}