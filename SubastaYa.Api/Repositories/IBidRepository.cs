using Microsoft.EntityFrameworkCore.Storage;
using SubastaYa.Api.Dtos.Bids;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Repositories;

public interface IBidRepository
{
    Task<IDbContextTransaction> BeginTransactionAsync();

    Task<Auction?> GetAuctionForBidAsync(int auctionId);

    Task<Wallet?> GetWalletByUserIdAsync(int userId);

    Task<Bid?> GetLeadingBidAsync(int auctionId);

    void AddBid(Bid bid);

    void AddTransactionLedger(TransactionLedger ledger);

    void AddAuditLog(AuditLog auditLog);

    Task SaveChangesAsync();

    Task CommitTransactionAsync(
        IDbContextTransaction transaction);

    Task RollbackTransactionAsync(
        IDbContextTransaction transaction);

    Task RegisterRejectedBidAsync(
        int auctionId,
        int buyerId,
        decimal amount,
        string reason);
}