namespace SubastaYa.Api.Models;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string AvatarUrl { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime RegisteredAtUtc { get; set; } = DateTime.UtcNow;

    public Wallet? Wallet { get; set; }

    public List<Auction> PublishedAuctions { get; set; } = [];

    public List<Bid> Bids { get; set; } = [];

    public List<AuditLog> AuditLogs { get; set; } = [];

}