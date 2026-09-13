using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Dtos.Bids;
using SubastaYa.Api.Helpers;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Repositories;

public class BidRepository : IBidRepository
{
    private readonly AppDbContext _context;

    public BidRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BidResultDto> PlaceBidAsync(
        int auctionId,
        CreateBidRequestDto request)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        Auction? auction = await _context.Auctions
            .FirstOrDefaultAsync(currentAuction =>
                currentAuction.Id == auctionId);

        if (auction is null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "la subasta indicada no existe.");
        }

        if (auction.SellerId == request.BuyerId)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "el vendedor no puede pujar en su propia subasta.");
        }

        DateTime nowUtc = DateTime.UtcNow;

        if (auction.Status == "PROGRAMADA" ||
            auction.StartAtUtc > nowUtc)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "la subasta todavía no comenzó.");
        }

        if (auction.EndAtUtc <= nowUtc)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "la subasta ya finalizó.");
        }

        if (auction.Status != "ACTIVA")
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                "la subasta no se encuentra activa.");
        }

        Wallet? buyerWallet = await _context.Wallets
            .FirstOrDefaultAsync(wallet =>
                wallet.UserId == request.BuyerId);

        if (buyerWallet is null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "la billetera del comprador no existe.");
        }

        Bid? previousLeadingBid = await _context.Bids
            .Where(bid => bid.AuctionId == auctionId)
            .OrderByDescending(bid => bid.Amount)
            .ThenByDescending(bid => bid.BidAtUtc)
            .FirstOrDefaultAsync();

        decimal minimumRequiredAmount = previousLeadingBid is null
            ? auction.BasePrice
            : previousLeadingBid.Amount + auction.MinimumIncrement;

        if (request.Amount < minimumRequiredAmount)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                $"la puja mínima permitida es {minimumRequiredAmount}.");
        }

        decimal reusableHeldAmount =
            previousLeadingBid?.BuyerId == request.BuyerId
                ? previousLeadingBid.Amount
                : 0;

        if (buyerWallet.AvailableBalance + reusableHeldAmount < request.Amount)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "el comprador no posee saldo disponible suficiente.");
        }

        if (previousLeadingBid is not null)
        {
            Wallet? previousBuyerWallet =
                previousLeadingBid.BuyerId == request.BuyerId
                    ? buyerWallet
                    : await _context.Wallets.FirstOrDefaultAsync(wallet =>
                        wallet.UserId == previousLeadingBid.BuyerId);

            if (previousBuyerWallet is null)
            {
                throw new ApiException(
                    StatusCodes.Status500InternalServerError,
                    "no se encontró la billetera del líder anterior.");
            }

            previousBuyerWallet.HeldBalance -= previousLeadingBid.Amount;
            previousBuyerWallet.AvailableBalance += previousLeadingBid.Amount;
            previousBuyerWallet.Version += 1;

            _context.TransactionLedgers.Add(new TransactionLedger
            {
                WalletId = previousBuyerWallet.Id,
                AuctionId = auction.Id,
                Type = "LIBERACION",
                Amount = previousLeadingBid.Amount,
                CreatedAtUtc = nowUtc
            });
        }

        buyerWallet.HeldBalance += request.Amount;
        buyerWallet.AvailableBalance -= request.Amount;
        buyerWallet.Version += 1;

        Bid newBid = new()
        {
            AuctionId = auction.Id,
            BuyerId = request.BuyerId,
            Amount = request.Amount,
            BidAtUtc = nowUtc
        };

        _context.Bids.Add(newBid);

        _context.TransactionLedgers.Add(new TransactionLedger
        {
            WalletId = buyerWallet.Id,
            AuctionId = auction.Id,
            Type = "RETENCION",
            Amount = request.Amount,
            CreatedAtUtc = nowUtc
        });

        bool antiSnipingApplied =
            auction.EndAtUtc - nowUtc <= TimeSpan.FromSeconds(60);

        if (antiSnipingApplied)
        {
            DateTime previousEndAtUtc = auction.EndAtUtc;

            auction.EndAtUtc = auction.EndAtUtc.AddMinutes(2);

            _context.AuditLogs.Add(new AuditLog
            {
                Entity = "SUBASTA",
                EntityId = auction.Id,
                Action = "EXTENSION_TIEMPO",
                UserId = request.BuyerId,
                DetailJson = JsonSerializer.Serialize(new
                {
                    previousEndAtUtc,
                    newEndAtUtc = auction.EndAtUtc
                }),
                CreatedAtUtc = nowUtc
            });
        }

        auction.Version += 1;

        _context.AuditLogs.Add(new AuditLog
        {
            Entity = "SUBASTA",
            EntityId = auction.Id,
            Action = "PUJA_REGISTRADA",
            UserId = request.BuyerId,
            DetailJson = JsonSerializer.Serialize(new
            {
                amount = request.Amount,
                bidAtUtc = nowUtc
            }),
            CreatedAtUtc = nowUtc
        });

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return new BidResultDto
        {
            BidId = newBid.Id,
            AuctionId = auction.Id,
            BuyerId = request.BuyerId,
            Amount = request.Amount,
            BidAtUtc = newBid.BidAtUtc,
            EndAtUtc = auction.EndAtUtc,
            AntiSnipingApplied = antiSnipingApplied
        };
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