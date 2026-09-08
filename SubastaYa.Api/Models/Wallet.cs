namespace SubastaYa.Api.Models;

public class Wallet
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public decimal TotalBalance { get; set; }

    public decimal HeldBalance { get; set; }

    public decimal AvailableBalance { get; set; }

    public int Version { get; set; } = 1;

    public User User { get; set; } = null!;

    public List<TransactionLedger> Transactions { get; set; } = [];
}