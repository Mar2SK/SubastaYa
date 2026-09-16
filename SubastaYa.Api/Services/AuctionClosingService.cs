using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Hubs;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Services;

public class AuctionClosingService : IAuctionClosingService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuctionClosingService> _logger;
    private readonly IHubContext<AuctionHub> _auctionHub;

    public AuctionClosingService(
        AppDbContext context,
        ILogger<AuctionClosingService> logger,
        IHubContext<AuctionHub> auctionHub)
    {
        _context = context;
        _logger = logger;
        _auctionHub = auctionHub;
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
                _context.ChangeTracker.Clear();

                await CloseAuctionAsync(
                    idx_tk,
                    DateTime.UtcNow);
            }
            catch (DbUpdateConcurrencyException exception)
            {
                _context.ChangeTracker.Clear();

                _logger.LogWarning(
                    exception,
                    "[CODE-ERROR] - conflicto de concurrencia al cerrar la subasta {AuctionId}.",
                    idx_tk);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "[CODE-ERROR] - error al procesar la subasta vencida {AuctionId}.",
                    idx_tk);
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
                currentAuction.Id == auctionId);

        if (auction is null)
        {
            await transaction.RollbackAsync();
            return;
        }

        if (auction.Status != "ACTIVA")
        {
            await transaction.RollbackAsync();
            return;
        }

        if (auction.EndAtUtc > nowUtc)
        {
            await transaction.RollbackAsync();
            return;
        }

        Bid? winnerBid = await _context.Bids
            .Where(bid =>
                bid.AuctionId == auction.Id)
            .OrderByDescending(bid =>
                bid.Amount)
            .ThenByDescending(bid =>
                bid.BidAtUtc)
            .FirstOrDefaultAsync();

        if (winnerBid is null)
        {
            auction.Status = "DESIERTA";
            auction.Version += 1;

            _context.AuditLogs.Add(
                new AuditLog
                {
                    Entity = "SUBASTA",
                    EntityId = auction.Id,
                    Action = "SUBASTA_DESIERTA",
                    UserId = null,
                    DetailJson =
                        "{\"motivo\":\"sin_pujas\"}",
                    CreatedAtUtc = nowUtc
                });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await NotifyAuctionClosedAsync(
                auction.Id,
                auction.Status);

            return;
        }

        bool paymentExists =
            await _context.TransactionLedgers
                .AnyAsync(item =>
                    item.AuctionId == auction.Id &&
                    item.Type == "PAGO");

        bool collectionExists =
            await _context.TransactionLedgers
                .AnyAsync(item =>
                    item.AuctionId == auction.Id &&
                    item.Type == "COBRO");

        if (paymentExists && collectionExists)
        {
            _logger.LogWarning(
                "La subasta {AuctionId} ya posee PAGO y COBRO. " +
                "Se corrige el estado a FINALIZADA sin volver a mover dinero.",
                auction.Id);

            auction.Status = "FINALIZADA";
            auction.Version += 1;

            _context.AuditLogs.Add(
                new AuditLog
                {
                    Entity = "SUBASTA",
                    EntityId = auction.Id,
                    Action = "ESTADO_REPARADO",
                    UserId = null,
                    DetailJson =
                        "{\"motivo\":\"liquidacion_existente\"}",
                    CreatedAtUtc = nowUtc
                });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            await NotifyAuctionClosedAsync(
                auction.Id,
                auction.Status);

            return;
        }

        if (paymentExists || collectionExists)
        {
            throw new InvalidOperationException(
            "[CODE-ERROR] - La subasta posee una liquidación parcial. Existe PAGO o COBRO, pero no ambos.");
        }

        Wallet? buyerWallet = await _context.Wallets
            .FirstOrDefaultAsync(wallet =>
                wallet.UserId == winnerBid.BuyerId);

        Wallet? sellerWallet = await _context.Wallets
            .FirstOrDefaultAsync(wallet =>
                wallet.UserId == auction.SellerId);

        if (buyerWallet is null ||
            sellerWallet is null)
        {
            throw new InvalidOperationException(
                "[CODE-ERROR] - No se encontraron las billeteras necesarias para liquidar la subasta.");
        }

        if (buyerWallet.HeldBalance < winnerBid.Amount)
        {
            decimal missingAmount =
                winnerBid.Amount - buyerWallet.HeldBalance;

            if (buyerWallet.AvailableBalance < missingAmount)
            {
                throw new InvalidOperationException(
                    "[CODE-ERROR] - el comprador no tiene fondos suficientes para liquidar la subasta.");
            }

            buyerWallet.AvailableBalance -= missingAmount;
            buyerWallet.HeldBalance += missingAmount;
        }

        buyerWallet.HeldBalance -=
            winnerBid.Amount;

        buyerWallet.TotalBalance -=
            winnerBid.Amount;

        buyerWallet.Version += 1;

        sellerWallet.TotalBalance +=
            winnerBid.Amount;

        sellerWallet.AvailableBalance +=
            winnerBid.Amount;

        sellerWallet.Version += 1;

        _context.TransactionLedgers.Add(
    new TransactionLedger
    {
        WalletId = sellerWallet.Id,
        AuctionId = auction.Id,
        Type = "COBRO",
        Amount = winnerBid.Amount,
        CreatedAtUtc = nowUtc
    });

        _context.Sales.Add(new Sale
        {
            AuctionId = auction.Id,
            BuyerId = winnerBid.BuyerId,
            SellerId = auction.SellerId,
            Amount = winnerBid.Amount,
            CreatedAtUtc = nowUtc
        });

        auction.Status = "FINALIZADA";
        auction.Version += 1;

        _context.AuditLogs.Add(
            new AuditLog
            {
                Entity = "SUBASTA",
                EntityId = auction.Id,
                Action = "CIERRE_WORKER",
                UserId = null,
                DetailJson =
                    $"{{\"compradorId\":{winnerBid.BuyerId}," +
                    $"\"monto\":{winnerBid.Amount}}}",
                CreatedAtUtc = nowUtc
            });

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        await NotifyAuctionClosedAsync(
            auction.Id,
            auction.Status);
    }

    private async Task NotifyAuctionClosedAsync(
        int auctionId,
        string status)
    {
        try
        {
            await _auctionHub.Clients
                .Group($"auction-{auctionId}")
                .SendAsync(
                    "AuctionClosed",
                    new
                    {
                        auctionId,
                        status
                    });
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "[CODE-ERROR] - no se pudo notificar el cierre de la subasta {AuctionId}.",
                auctionId);
        }
    }
}