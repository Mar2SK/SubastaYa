using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Api.Dtos.Bids;

public class CreateBidRequestDto
{
    [Required]
    public required int BuyerId { get; set; }

    [Required]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public required decimal Amount { get; set; }
}