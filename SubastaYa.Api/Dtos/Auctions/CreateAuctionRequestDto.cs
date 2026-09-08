namespace SubastaYa.Api.Dtos.Auctions;

public class CreateAuctionRequestDto
{
    public int SellerId { get; set; }

    public int CategoryId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    public decimal BasePrice { get; set; }

    public decimal MinimumIncrement { get; set; }

    public DateTime StartAtUtc { get; set; }

    public DateTime EndAtUtc { get; set; }
}