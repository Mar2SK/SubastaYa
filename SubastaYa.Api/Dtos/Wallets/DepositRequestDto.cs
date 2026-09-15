using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Api.Dtos.Wallets;

public class DepositRequestDto
{
    [Required]
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Amount { get; set; }
}