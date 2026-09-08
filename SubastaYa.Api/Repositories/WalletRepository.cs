using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Models;
using SubastaYa.Api.Dtos.Wallets;

namespace SubastaYa.Api.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly AppDbContext _context;

    public WalletRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Wallet?> GetByUserIdAsync(int userId)
    {
        return await _context.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(wallet => wallet.UserId == userId);
    }

    public async Task<Wallet?> DepositAsync(int userId, decimal amount)
    {
        Wallet? wallet = await _context.Wallets
            .FirstOrDefaultAsync(wallet => wallet.UserId == userId);

        if (wallet is null)
        {
            return null;
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            wallet.TotalBalance += amount;
            wallet.AvailableBalance += amount;
            wallet.Version += 1;

            _context.TransactionLedgers.Add(new TransactionLedger
            {
                WalletId = wallet.Id,
                Type = "DEPOSITO",
                Amount = amount,
                CreatedAtUtc = DateTime.UtcNow
            });

            _context.AuditLogs.Add(new AuditLog
            {
                Entity = "BILLETERA",
                EntityId = wallet.Id,
                Action = "ACREDITACION_MANUAL",
                UserId = wallet.UserId,
                DetailJson = $"{{\"monto\":{amount}}}",
                CreatedAtUtc = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return wallet;
        }
        catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }
    public async Task<List<WalletTransactionItemDto>>
    GetTransactionsByUserIdAsync(int userId)
    {
        return await _context.TransactionLedgers
            .AsNoTracking()
            .Where(transaction => transaction.Wallet.UserId == userId)
            .OrderByDescending(transaction => transaction.CreatedAtUtc)
            .Select(transaction => new WalletTransactionItemDto
            {
                Id = transaction.Id,
                Type = transaction.Type,
                Amount = transaction.Amount,
                CreatedAtUtc = transaction.CreatedAtUtc,
                AuctionId = transaction.AuctionId
            })
            .ToListAsync();
    }
}