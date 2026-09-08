namespace SubastaYa.Api.Dtos.Auctions;

public class AuctionListItemDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal BasePrice { get; set; }

    public decimal? HighestBid { get; set; }

    public int BidCount { get; set; }

    public DateTime EndAtUtc { get; set; }
}