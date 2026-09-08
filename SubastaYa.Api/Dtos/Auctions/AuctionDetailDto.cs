namespace SubastaYa.Api.Dtos.Auctions;

public class AuctionDetailDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public string SellerName { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal BasePrice { get; set; }

    public decimal MinimumIncrement { get; set; }

    public decimal? HighestBid { get; set; }

    public DateTime StartAtUtc { get; set; }

    public DateTime EndAtUtc { get; set; }

    public List<BidHistoryItemDto> Bids { get; set; } = [];
}