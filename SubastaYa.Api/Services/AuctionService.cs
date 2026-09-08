using SubastaYa.Api.Dtos;
using SubastaYa.Api.Dtos.Auctions;
using SubastaYa.Api.Helpers;
using SubastaYa.Api.Models;
using SubastaYa.Api.Repositories;

namespace SubastaYa.Api.Services;

public class AuctionService : IAuctionService
{
    private readonly IAuctionRepository _auctionRepository;

    public AuctionService(IAuctionRepository auctionRepository)
    {
        _auctionRepository = auctionRepository;
    }

    public async Task<PagedResponseDto<AuctionListItemDto>> GetAllAsync(
        AuctionQueryDto query)
    {
        return await _auctionRepository.GetAllAsync(query);
    }

    public async Task<AuctionDetailDto?> GetByIdAsync(int id)
    {
        return await _auctionRepository.GetByIdAsync(id);
    }

    public async Task<AuctionDetailDto> CreateAsync(
    CreateAuctionRequestDto request)
    {
        if (!await _auctionRepository.SellerExistsAsync(request.SellerId))
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "el vendedor indicado no existe.");
        }

        if (!await _auctionRepository.CategoryExistsAsync(request.CategoryId))
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "la categoría indicada no existe.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "el título es obligatorio.");
        }

        if (request.BasePrice <= 0)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "el precio base debe ser mayor a cero.");
        }

        if (request.MinimumIncrement <= 0)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "el incremento mínimo debe ser mayor a cero.");
        }

        if (request.EndAtUtc <= request.StartAtUtc)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "la fecha de finalización debe ser posterior al inicio.");
        }

        if (request.EndAtUtc <= DateTime.UtcNow)
        {
            throw new ApiException(
                StatusCodes.Status400BadRequest,
                "la fecha de finalización debe estar en el futuro.");
        }

        string status = request.StartAtUtc > DateTime.UtcNow
            ? "PROGRAMADA"
            : "ACTIVA";

        Auction auction = new()
        {
            SellerId = request.SellerId,
            CategoryId = request.CategoryId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            ImageUrl = request.ImageUrl.Trim(),
            BasePrice = request.BasePrice,
            MinimumIncrement = request.MinimumIncrement,
            StartAtUtc = request.StartAtUtc,
            EndAtUtc = request.EndAtUtc,
            Status = status,
            Version = 1
        };

        Auction createdAuction = await _auctionRepository.CreateAsync(auction);

        AuctionDetailDto? detail =
            await _auctionRepository.GetByIdAsync(createdAuction.Id);

        if (detail is null)
        {
            throw new ApiException(
                StatusCodes.Status500InternalServerError,
                "no se pudo recuperar la subasta creada.");
        }

        return detail;
    }
}