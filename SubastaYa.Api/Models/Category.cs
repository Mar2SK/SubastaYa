namespace SubastaYa.Api.Models;

public class Category
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string IconUrl { get; set; } = string.Empty;

    public List<Auction> Auctions { get; set; } = [];
}