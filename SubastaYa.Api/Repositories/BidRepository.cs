using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SubastaYa.Api.Data;
using SubastaYa.Api.Dtos.Bids;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Repositories;

public class BidRepository : IBidRepository
{
    private readonly AppDbContext _context;

    public BidRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return _context.Database.BeginTransactionAsync();
    }

    public Task<Auction?> GetAuctionForBidAsync(int auctionId)
    {
        return _context.Auctions.FirstOrDefaultAsync(
            auction => auction.Id == auctionId);
    }

    public Task<Wallet?> GetWalletByUserIdAsync(int userId)
    {
        return _context.Wallets.FirstOrDefaultAsync(
            wallet => wallet.UserId == userId);
    }

    public Task<Bid?> GetLeadingBidAsync(int auctionId)
    {
        return _context.Bids
            .Where(bid => bid.AuctionId == auctionId)
            .OrderByDescending(bid => bid.Amount)
            .ThenByDescending(bid => bid.BidAtUtc)
            .FirstOrDefaultAsync();
    }

    public void AddBid(Bid bid)
    {
        _context.Bids.Add(bid);
    }

    public void AddTransactionLedger(TransactionLedger ledger)
    {
        _context.TransactionLedgers.Add(ledger);
    }

    public void AddAuditLog(AuditLog auditLog)
    {
        _context.AuditLogs.Add(auditLog);
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    public Task CommitTransactionAsync(
        IDbContextTransaction transaction)
    {
        return transaction.CommitAsync();
    }

    public Task RollbackTransactionAsync(
        IDbContextTransaction transaction)
    {
        return transaction.RollbackAsync();
    }

    public async Task RegisterRejectedBidAsync(
        int auctionId,
        int buyerId,
        decimal amount,
        string reason)
    {
        _context.ChangeTracker.Clear();

        bool buyerExists = await _context.Users
            .AnyAsync(user => user.Id == buyerId);

        _context.AuditLogs.Add(new AuditLog
        {
            Entity = "SUBASTA",
            EntityId = auctionId,
            Action = "PUJA_RECHAZADA",
            UserId = buyerExists ? buyerId : null,
            DetailJson = JsonSerializer.Serialize(new
            {
                amount,
                reason
            }),
            CreatedAtUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }
}