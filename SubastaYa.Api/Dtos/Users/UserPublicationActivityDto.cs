namespace SubastaYa.Api.Dtos.Users;

public class UserPublicationActivityDto
{
    public int AuctionId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int BidCount { get; set; }

    public decimal? HighestBid { get; set; }

    public DateTime EndAtUtc { get; set; }
}