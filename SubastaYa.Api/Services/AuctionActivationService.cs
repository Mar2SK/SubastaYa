using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Services;

public class AuctionActivationService : IAuctionActivationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuctionActivationService> _logger;

    public AuctionActivationService(
        AppDbContext context,
        ILogger<AuctionActivationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ProcessScheduledAuctionsAsync()
    {
        DateTime nowUtc = DateTime.UtcNow;

        List<Auction> auctions = await _context.Auctions
            .Where(auction =>
                auction.Status == "PROGRAMADA" &&
                auction.StartAtUtc <= nowUtc &&
                auction.EndAtUtc > nowUtc)
            .ToListAsync();

        if (auctions.Count == 0)
        {
            return;
        }

        foreach (Auction auction in auctions)
        {
            auction.Status = "ACTIVA";
            auction.Version += 1;

            _context.AuditLogs.Add(new AuditLog
            {
                Entity = "SUBASTA",
                EntityId = auction.Id,
                Action = "SUBASTA_ACTIVADA",
                DetailJson =
                    $"{{\"startAtUtc\":\"{auction.StartAtUtc:O}\"}}",
                CreatedAtUtc = nowUtc
            });
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Se activaron {Count} subastas programadas.",
            auctions.Count);
    }
}