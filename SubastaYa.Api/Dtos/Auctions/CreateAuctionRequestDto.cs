using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Api.Dtos.Auctions;

public class CreateAuctionRequestDto
{
    [Required]
    public required int SellerId { get; set; }

    [Required]
    public required int CategoryId { get; set; }

    [Required]
    [MaxLength(150)]
    public required string Title { get; set; }

    [Required]
    [MaxLength(1000)]
    public required string Description { get; set; }

    [Required]
    public required string ImageUrl { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public required decimal BasePrice { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public required decimal MinimumIncrement { get; set; }

    [Required]
    public required DateTime StartAtUtc { get; set; }

    [Required]
    public required DateTime EndAtUtc { get; set; }
}