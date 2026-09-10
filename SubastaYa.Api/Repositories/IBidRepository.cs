using SubastaYa.Api.Dtos.Bids;

namespace SubastaYa.Api.Repositories;

public interface IBidRepository
{
    Task<BidResultDto> PlaceBidAsync(
        int auctionId,
        CreateBidRequestDto request);

    Task RegisterRejectedBidAsync(
        int auctionId,
        int buyerId,
        decimal amount,
        string reason);
}