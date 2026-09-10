namespace SubastaYa.Api.Dtos.Wallets;

public class WalletBalanceDto
{
    public int WalletId { get; set; }

    public int UserId { get; set; }

    public decimal TotalBalance { get; set; }

    public decimal HeldBalance { get; set; }

    public decimal AvailableBalance { get; set; }
}