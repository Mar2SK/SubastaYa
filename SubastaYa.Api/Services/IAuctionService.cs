using SubastaYa.Api.Dtos;
using SubastaYa.Api.Dtos.Auctions;
using SubastaYa.Api.Helpers;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Services;

public interface IAuctionService
{
    Task<PagedResponseDto<AuctionListItemDto>> GetAllAsync(
        AuctionQueryDto query);

    Task<AuctionDetailDto?> GetByIdAsync(int id);

    Task<AuctionDetailDto> CreateAsync(
    CreateAuctionRequestDto request);

}