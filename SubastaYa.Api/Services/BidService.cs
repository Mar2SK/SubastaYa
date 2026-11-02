using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SubastaYa.Api.Dtos.Bids;
using SubastaYa.Api.Helpers;
using SubastaYa.Api.Hubs;
using SubastaYa.Api.Models;
using SubastaYa.Api.Repositories;

namespace SubastaYa.Api.Services;

public class BidService : IBidService
{
    private readonly IBidRepository _bidRepository;
    private readonly IHubContext<AuctionHub> _auctionHub;
    private readonly ILogger<BidService> _logger;
    private readonly BidBusinessRules _businessRules;

    public BidService(
        IBidRepository bidRepository,
        IHubContext<AuctionHub> auctionHub,
        ILogger<BidService> logger,
        BidBusinessRules businessRules)
    {
        _bidRepository = bidRepository;
        _auctionHub = auctionHub;
        _logger = logger;
        _businessRules = businessRules;
    }

    public async Task<BidResultDto> CreateAsync(
        int auctionId,
        CreateBidRequestDto request)
    {
        IDbContextTransaction? transaction = null;

        try
        {
            _businessRules.ValidatePositiveAmount(request.Amount);

            transaction = await _bidRepository.BeginTransactionAsync();

            Auction? auction =
                await _bidRepository.GetAuctionForBidAsync(auctionId);

            if (auction is null)
            {
                throw new ApiException(
                    StatusCodes.Status404NotFound,
                    "la subasta indicada no existe.");
            }

            DateTime nowUtc = DateTime.UtcNow;

            _businessRules.ValidateAuction(
                auction,
                request.BuyerId,
                nowUtc);

            Wallet? buyerWallet =
                await _bidRepository.GetWalletByUserIdAsync(
                    request.BuyerId);

            if (buyerWallet is null)
            {
                throw new ApiException(
                    StatusCodes.Status404NotFound,
                    "la billetera del comprador no existe.");
            }

            Bid? previousLeadingBid =
                await _bidRepository.GetLeadingBidAsync(auctionId);

            decimal minimumRequiredAmount =
                _businessRules.GetMinimumRequiredAmount(
                    auction,
                    previousLeadingBid);

            _businessRules.ValidateAmount(
                request.Amount,
                minimumRequiredAmount);

            _businessRules.ValidateAvailableBalance(
                buyerWallet,
                previousLeadingBid,
                request.BuyerId,
                request.Amount);

            if (previousLeadingBid is not null)
            {
                Wallet? previousBuyerWallet =
                    previousLeadingBid.BuyerId == request.BuyerId
                        ? buyerWallet
                        : await _bidRepository.GetWalletByUserIdAsync(
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

                _bidRepository.AddTransactionLedger(
                    new TransactionLedger
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

            _bidRepository.AddBid(newBid);

            _bidRepository.AddTransactionLedger(
                new TransactionLedger
                {
                    WalletId = buyerWallet.Id,
                    AuctionId = auction.Id,
                    Type = "RETENCION",
                    Amount = request.Amount,
                    CreatedAtUtc = nowUtc
                });

            bool antiSnipingApplied =
                _businessRules.ShouldApplyAntiSniping(
                    auction,
                    nowUtc);

            if (antiSnipingApplied)
            {
                DateTime previousEndAtUtc = auction.EndAtUtc;

                auction.EndAtUtc = auction.EndAtUtc.AddMinutes(2);

                _bidRepository.AddAuditLog(new AuditLog
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

            _bidRepository.AddAuditLog(new AuditLog
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

            await _bidRepository.SaveChangesAsync();

            await _bidRepository.CommitTransactionAsync(transaction);

            BidResultDto result = new()
            {
                BidId = newBid.Id,
                AuctionId = auction.Id,
                BuyerId = request.BuyerId,
                Amount = request.Amount,
                BidAtUtc = newBid.BidAtUtc,
                EndAtUtc = auction.EndAtUtc,
                AntiSnipingApplied = antiSnipingApplied
            };

            await NotifyBidAsync(result);

            return result;
        }

        catch (ApiException exception)
        {
            await RollbackAndDisposeSafelyAsync(transaction);
            transaction = null;

            try
            {
                await _bidRepository.RegisterRejectedBidAsync(
                    auctionId,
                    request.BuyerId,
                    request.Amount,
                    exception.Message);
            }
            catch (Exception auditException)
            {
                _logger.LogError(
                    auditException,
                    "[CODE-ERROR] - no se pudo registrar la auditoría de la puja rechazada.");
            }

            throw;
        }
        catch (DbUpdateConcurrencyException)
        {
            await RollbackAndDisposeSafelyAsync(transaction);
            transaction = null;

            try
            {
                await _bidRepository.RegisterRejectedBidAsync(
                    auctionId,
                    request.BuyerId,
                    request.Amount,
                    "CONFLICTO_CONCURRENCIA");
            }
            catch (Exception auditException)
            {
                _logger.LogError(
                    auditException,
                    "[CODE-ERROR] - no se pudo registrar la auditoría del conflicto de concurrencia.");
            }

            throw;
        }
        catch
        {
            await RollbackAndDisposeSafelyAsync(transaction);
            transaction = null;
            throw;
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }

    }


    private async Task RollbackAndDisposeSafelyAsync(
        IDbContextTransaction? transaction)
    {
        if (transaction is null)
        {
            return;
        }

        try
        {
            await _bidRepository.RollbackTransactionAsync(transaction);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "[CODE-ERROR] - no se pudo revertir la transacción de la puja.");
        }
        finally
        {
            try
            {
                await transaction.DisposeAsync();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "[CODE-ERROR] - no se pudo liberar la transacción de la puja.");
            }
        }
    }


    private async Task NotifyBidAsync(BidResultDto result)
    {
        try
        {
            await _auctionHub.Clients
                .Group($"auction-{result.AuctionId}")
                .SendAsync("BidPlaced", result);

            if (result.AntiSnipingApplied)
            {
                await _auctionHub.Clients
                    .Group($"auction-{result.AuctionId}")
                    .SendAsync("AuctionExtended", new
                    {
                        auctionId = result.AuctionId,
                        endAtUtc = result.EndAtUtc
                    });
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "[CODE-ERROR] - no se pudo enviar la notificación de SignalR.");
        }
    }
}