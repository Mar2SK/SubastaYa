using SubastaYa.Api.Dtos.Bids;

namespace SubastaYa.Api.Services;

public interface IBidService
{
    Task<BidResultDto> CreateAsync(
        int auctionId,
        CreateBidRequestDto request);
}