namespace SubastaYa.Api.Dtos.Users;

public class UserBidActivityDto
{
    public int AuctionId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public decimal MyHighestBid { get; set; }

    public decimal? CurrentHighestBid { get; set; }

    public bool IsLeading { get; set; }

    public bool HasWon { get; set; }

    public DateTime EndAtUtc { get; set; }
}