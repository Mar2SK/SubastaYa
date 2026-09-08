using SubastaYa.Api.Dtos;
using SubastaYa.Api.Dtos.Auctions;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Repositories;

public interface IAuctionRepository
{
    Task<PagedResponseDto<AuctionListItemDto>> GetAllAsync(
        AuctionQueryDto query);

    Task<AuctionDetailDto?> GetByIdAsync(int id);

    Task<bool> SellerExistsAsync(int sellerId);

    Task<bool> CategoryExistsAsync(int categoryId);

    Task<Auction> CreateAsync(Auction auction);
}