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

        Auction auction = await GetAuctionAsync(auctionId);

        DateTime nowUtc = DateTime.UtcNow;

        ValidateAuction(
            auction,
            request.BuyerId,
            nowUtc);

        Wallet buyerWallet =
            await GetBuyerWalletAsync(request.BuyerId);

        Bid? previousLeadingBid =
            await GetPreviousLeadingBidAsync(auctionId);

        ValidateBidAmount(
            auction,
            previousLeadingBid,
            request.Amount);

        ValidateAvailableBalance(
            buyerWallet,
            previousLeadingBid,
            request);

        if (previousLeadingBid is not null)
        {
            await ReleasePreviousBidAsync(
                auction,
                previousLeadingBid,
                buyerWallet,
                nowUtc);
        }

        Bid newBid = RegisterNewBid(
            auction,
            buyerWallet,
            request,
            nowUtc);

        bool antiSnipingApplied =
            ApplyAntiSnipingIfNecessary(
                auction,
                request.BuyerId,
                nowUtc);

        auction.Version += 1;

        RegisterBidAudit(
            auction,
            request,
            nowUtc);

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

    private async Task<Auction> GetAuctionAsync(int auctionId)
    {
        Auction? auction = await _context.Auctions
            .FirstOrDefaultAsync(currentAuction =>
                currentAuction.Id == auctionId);

        if (auction is null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "la subasta indicada no existe.");
        }

        return auction;
    }

    private static void ValidateAuction(
        Auction auction,
        int buyerId,
        DateTime nowUtc)
    {
        if (auction.SellerId == buyerId)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "el vendedor no puede pujar en su propia subasta.");
        }

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
    }

    private async Task<Wallet> GetBuyerWalletAsync(int buyerId)
    {
        Wallet? buyerWallet = await _context.Wallets
            .FirstOrDefaultAsync(wallet =>
                wallet.UserId == buyerId);

        if (buyerWallet is null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "la billetera del comprador no existe.");
        }

        return buyerWallet;
    }

    private async Task<Bid?> GetPreviousLeadingBidAsync(
        int auctionId)
    {
        return await _context.Bids
            .Where(bid => bid.AuctionId == auctionId)
            .OrderByDescending(bid => bid.Amount)
            .ThenByDescending(bid => bid.BidAtUtc)
            .FirstOrDefaultAsync();
    }

    private static void ValidateBidAmount(
        Auction auction,
        Bid? previousLeadingBid,
        decimal amount)
    {
        decimal minimumRequiredAmount =
            previousLeadingBid is null
                ? auction.BasePrice
                : previousLeadingBid.Amount +
                  auction.MinimumIncrement;

        if (amount < minimumRequiredAmount)
        {
            throw new ApiException(
                StatusCodes.Status409Conflict,
                $"la puja mínima permitida es {minimumRequiredAmount}.");
        }
    }

    private static void ValidateAvailableBalance(
        Wallet buyerWallet,
        Bid? previousLeadingBid,
        CreateBidRequestDto request)
    {
        decimal reusableHeldAmount =
            previousLeadingBid?.BuyerId == request.BuyerId
                ? previousLeadingBid.Amount
                : 0;

        if (buyerWallet.AvailableBalance +
            reusableHeldAmount < request.Amount)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "el comprador no posee saldo disponible suficiente.");
        }
    }

    private async Task ReleasePreviousBidAsync(
        Auction auction,
        Bid previousLeadingBid,
        Wallet buyerWallet,
        DateTime nowUtc)
    {
        Wallet? previousBuyerWallet =
            previousLeadingBid.BuyerId == buyerWallet.UserId
                ? buyerWallet
                : await _context.Wallets
                    .FirstOrDefaultAsync(wallet =>
                        wallet.UserId ==
                        previousLeadingBid.BuyerId);

        if (previousBuyerWallet is null)
        {
            throw new ApiException(
                StatusCodes.Status500InternalServerError,
                "no se encontró la billetera del líder anterior.");
        }

        previousBuyerWallet.HeldBalance -=
            previousLeadingBid.Amount;

        previousBuyerWallet.AvailableBalance +=
            previousLeadingBid.Amount;

        previousBuyerWallet.Version += 1;

        _context.TransactionLedgers.Add(
            new TransactionLedger
            {
                WalletId = previousBuyerWallet.Id,
                AuctionId = auction.Id,
                Type = "LIBERACION",
                Amount = previousLeadingBid.Amount,
                CreatedAtUtc = nowUtc
            });
    }

    private Bid RegisterNewBid(
        Auction auction,
        Wallet buyerWallet,
        CreateBidRequestDto request,
        DateTime nowUtc)
    {
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

        _context.TransactionLedgers.Add(
            new TransactionLedger
            {
                WalletId = buyerWallet.Id,
                AuctionId = auction.Id,
                Type = "RETENCION",
                Amount = request.Amount,
                CreatedAtUtc = nowUtc
            });

        return newBid;
    }

    private bool ApplyAntiSnipingIfNecessary(
        Auction auction,
        int buyerId,
        DateTime nowUtc)
    {
        bool antiSnipingApplied =
            auction.EndAtUtc - nowUtc <=
            TimeSpan.FromSeconds(60);

        if (!antiSnipingApplied)
        {
            return false;
        }

        DateTime previousEndAtUtc = auction.EndAtUtc;

        auction.EndAtUtc =
            auction.EndAtUtc.AddMinutes(2);

        _context.AuditLogs.Add(
            new AuditLog
            {
                Entity = "SUBASTA",
                EntityId = auction.Id,
                Action = "EXTENSION_TIEMPO",
                UserId = buyerId,
                DetailJson = JsonSerializer.Serialize(new
                {
                    previousEndAtUtc,
                    newEndAtUtc = auction.EndAtUtc
                }),
                CreatedAtUtc = nowUtc
            });

        return true;
    }

    private void RegisterBidAudit(
        Auction auction,
        CreateBidRequestDto request,
        DateTime nowUtc)
    {
        _context.AuditLogs.Add(
            new AuditLog
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

        _context.AuditLogs.Add(
            new AuditLog
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