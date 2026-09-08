using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Dtos.Bids;
using SubastaYa.Api.Helpers;
using SubastaYa.Api.Hubs;
using SubastaYa.Api.Repositories;

namespace SubastaYa.Api.Services;

public class BidService : IBidService
{
    private readonly IBidRepository _bidRepository;
    private readonly IHubContext<AuctionHub> _auctionHub;
    private readonly ILogger<BidService> _logger;

    public BidService(
        IBidRepository bidRepository,
        IHubContext<AuctionHub> auctionHub,
        ILogger<BidService> logger)
    {
        _bidRepository = bidRepository;
        _auctionHub = auctionHub;
        _logger = logger;
    }

    public async Task<BidResultDto> CreateAsync(
        int auctionId,
        CreateBidRequestDto request)
    {
        try
        {
            if (request.Amount <= 0)
            {
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "el monto de la puja debe ser mayor a cero.");
            }

            BidResultDto result = await _bidRepository.PlaceBidAsync(
                auctionId,
                request);

            await NotifyBidAsync(result);

            return result;
        }
        catch (ApiException exception)
        {
            await _bidRepository.RegisterRejectedBidAsync(
                auctionId,
                request.BuyerId,
                request.Amount,
                exception.Message);

            throw;
        }
        catch (DbUpdateConcurrencyException)
        {
            await _bidRepository.RegisterRejectedBidAsync(
                auctionId,
                request.BuyerId,
                request.Amount,
                "CONFLICTO_CONCURRENCIA");

            throw;
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