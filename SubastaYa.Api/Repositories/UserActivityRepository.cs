using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Dtos.Users;

namespace SubastaYa.Api.Repositories;

public class UserActivityRepository : IUserActivityRepository
{
    private readonly AppDbContext _context;

    public UserActivityRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> UserExistsAsync(int userId)
    {
        return await _context.Users.AnyAsync(user => user.Id == userId);
    }

    public async Task<List<UserBidActivityDto>> GetBidActivitiesAsync(
        int userId)
    {
        return await _context.Auctions
            .AsNoTracking()
            .Where(auction => auction.Bids.Any(bid =>
                bid.BuyerId == userId))
            .OrderByDescending(auction => auction.EndAtUtc)
            .Select(auction => new UserBidActivityDto
            {
                AuctionId = auction.Id,
                Title = auction.Title,
                ImageUrl = auction.ImageUrl,
                Status = auction.Status,
                MyHighestBid = auction.Bids
                    .Where(bid => bid.BuyerId == userId)
                    .Max(bid => bid.Amount),
                CurrentHighestBid = auction.Bids
                    .Max(bid => (decimal?)bid.Amount),
                IsLeading = auction.Bids
                    .OrderByDescending(bid => bid.Amount)
                    .ThenByDescending(bid => bid.BidAtUtc)
                    .Select(bid => bid.BuyerId)
                    .FirstOrDefault() == userId,
                HasWon = auction.Status == "FINALIZADA" &&
                    auction.Bids
                        .OrderByDescending(bid => bid.Amount)
                        .ThenByDescending(bid => bid.BidAtUtc)
                        .Select(bid => bid.BuyerId)
                        .FirstOrDefault() == userId,
                EndAtUtc = auction.EndAtUtc
            })
            .ToListAsync();
    }

    public async Task<List<UserPublicationActivityDto>>
        GetPublicationActivitiesAsync(int userId)
    {
        return await _context.Auctions
            .AsNoTracking()
            .Where(auction => auction.SellerId == userId)
            .OrderByDescending(auction => auction.EndAtUtc)
            .Select(auction => new UserPublicationActivityDto
            {
                AuctionId = auction.Id,
                Title = auction.Title,
                ImageUrl = auction.ImageUrl,
                Status = auction.Status,
                BidCount = auction.Bids.Count,
                HighestBid = auction.Bids
                    .Max(bid => (decimal?)bid.Amount),
                EndAtUtc = auction.EndAtUtc
            })
            .ToListAsync();
    }
}