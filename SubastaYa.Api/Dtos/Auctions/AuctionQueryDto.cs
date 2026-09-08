namespace SubastaYa.Api.Dtos.Auctions;

public class AuctionQueryDto
{
    public string? Status { get; set; }

    public int? CategoryId { get; set; }

    public decimal? MinimumPrice { get; set; }

    public decimal? MaximumPrice { get; set; }

    public string? OrderBy { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}