using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Services;

public class AuctionClosingService : IAuctionClosingService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuctionClosingService> _logger;

    public AuctionClosingService(
        AppDbContext context,
        ILogger<AuctionClosingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ProcessExpiredAuctionsAsync()
    {
        DateTime nowUtc = DateTime.UtcNow;

        List<int> expiredAuctionIds = await _context.Auctions
            .AsNoTracking()
            .Where(auction =>
                auction.Status == "ACTIVA" &&
                auction.EndAtUtc <= nowUtc)
            .Select(auction => auction.Id)
            .ToListAsync();

        foreach (int idx_tk in expiredAuctionIds)
        {
            try
            {
                await CloseAuctionAsync(idx_tk, nowUtc);
            }
            catch (DbUpdateConcurrencyException exception)
            {
                _logger.LogWarning(
                    exception,
                    "[CODE-ERROR] - conflicto de concurrencia al cerrar una subasta.");
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "[CODE-ERROR] - error al procesar una subasta vencida.");
            }
        }
    }

    private async Task CloseAuctionAsync(
        int auctionId,
        DateTime nowUtc)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        Auction? auction = await _context.Auctions
            .FirstOrDefaultAsync(currentAuction =>
                currentAuction.Id == auctionId &&
                currentAuction.Status == "ACTIVA");

        if (auction is null)
        {
            return;
        }

        Bid? winnerBid = await _context.Bids
            .Where(bid => bid.AuctionId == auction.Id)
            .OrderByDescending(bid => bid.Amount)
            .ThenByDescending(bid => bid.BidAtUtc)
            .FirstOrDefaultAsync();

        if (winnerBid is null)
        {
            auction.Status = "DESIERTA";
            auction.Version += 1;

            _context.AuditLogs.Add(new AuditLog
            {
                Entity = "SUBASTA",
                EntityId = auction.Id,
                Action = "SUBASTA_DESIERTA",
                UserId = null,
                DetailJson = "{\"motivo\":\"sin_pujas\"}",
                CreatedAtUtc = nowUtc
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return;
        }

        Wallet? buyerWallet = await _context.Wallets
            .FirstOrDefaultAsync(wallet =>
                wallet.UserId == winnerBid.BuyerId);

        Wallet? sellerWallet = await _context.Wallets
            .FirstOrDefaultAsync(wallet =>
                wallet.UserId == auction.SellerId);

        if (buyerWallet is null || sellerWallet is null)
        {
            throw new InvalidOperationException(
                "No se encontraron las billeteras necesarias para liquidar la subasta.");
        }

        if (buyerWallet.HeldBalance < winnerBid.Amount)
        {
            throw new InvalidOperationException(
                "El saldo retenido no alcanza para liquidar la subasta.");
        }

        buyerWallet.HeldBalance -= winnerBid.Amount;
        buyerWallet.TotalBalance -= winnerBid.Amount;
        buyerWallet.Version += 1;

        sellerWallet.TotalBalance += winnerBid.Amount;
        sellerWallet.AvailableBalance += winnerBid.Amount;
        sellerWallet.Version += 1;

        _context.TransactionLedgers.Add(new TransactionLedger
        {
            WalletId = buyerWallet.Id,
            AuctionId = auction.Id,
            Type = "PAGO",
            Amount = winnerBid.Amount,
            CreatedAtUtc = nowUtc
        });

        _context.TransactionLedgers.Add(new TransactionLedger
        {
            WalletId = sellerWallet.Id,
            AuctionId = auction.Id,
            Type = "COBRO",
            Amount = winnerBid.Amount,
            CreatedAtUtc = nowUtc
        });

        auction.Status = "FINALIZADA";
        auction.Version += 1;

        _context.AuditLogs.Add(new AuditLog
        {
            Entity = "SUBASTA",
            EntityId = auction.Id,
            Action = "CIERRE_WORKER",
            UserId = null,
            DetailJson =
                $"{{\"compradorId\":{winnerBid.BuyerId},\"monto\":{winnerBid.Amount}}}",
            CreatedAtUtc = nowUtc
        });

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();
    }
}