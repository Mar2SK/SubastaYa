using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Dtos;
using SubastaYa.Api.Dtos.Auctions;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Repositories;

public class AuctionRepository : IAuctionRepository
{
    private readonly AppDbContext _context;

    public AuctionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResponseDto<AuctionListItemDto>> GetAllAsync(
        AuctionQueryDto query)
    {
        IQueryable<Auction> auctions = _context.Auctions
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            auctions = auctions.Where(auction =>
                auction.Status == query.Status.ToUpper());
        }

        if (query.CategoryId.HasValue)
        {
            auctions = auctions.Where(auction =>
                auction.CategoryId == query.CategoryId.Value);
        }

        if (query.MinimumPrice.HasValue)
        {
            auctions = auctions.Where(auction =>
                auction.BasePrice >= query.MinimumPrice.Value);
        }

        if (query.MaximumPrice.HasValue)
        {
            auctions = auctions.Where(auction =>
                auction.BasePrice <= query.MaximumPrice.Value);
        }

        auctions = query.OrderBy?.ToLower() switch
        {
            "mayor-puja" => auctions.OrderByDescending(auction =>
                auction.Bids.Max(bid => (decimal?)bid.Amount)),
            "menor-tiempo" => auctions.OrderBy(auction =>
                auction.EndAtUtc),
            _ => auctions.OrderBy(auction => auction.EndAtUtc)
        };

        int totalItems = await auctions.CountAsync();

        int safePage = query.Page < 1 ? 1 : query.Page;
        int safePageSize = query.PageSize < 1 ? 10 : query.PageSize;
        safePageSize = safePageSize > 50 ? 50 : safePageSize;

        List<AuctionListItemDto> items = await auctions
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(auction => new AuctionListItemDto
            {
                Id = auction.Id,
                Title = auction.Title,
                ImageUrl = auction.ImageUrl,
                CategoryName = auction.Category.Name,
                Status = auction.Status,
                BasePrice = auction.BasePrice,
                HighestBid = auction.Bids.Max(bid => (decimal?)bid.Amount),
                BidCount = auction.Bids.Count,
                StartAtUtc = DateTime.SpecifyKind(
                auction.StartAtUtc,
                DateTimeKind.Utc),

                EndAtUtc = DateTime.SpecifyKind(
                            auction.EndAtUtc,
                            DateTimeKind.Utc),
                })
                .ToListAsync();

        return new PagedResponseDto<AuctionListItemDto>
        {
            Page = safePage,
            PageSize = safePageSize,
            TotalItems = totalItems,
            Items = items
        };
    }

    public async Task<AuctionDetailDto?> GetByIdAsync(int id)
    {
        return await _context.Auctions
            .AsNoTracking()
            .Where(auction => auction.Id == id)
            .Select(auction => new AuctionDetailDto
            {
                Id = auction.Id,
                Title = auction.Title,
                Description = auction.Description,
                ImageUrl = auction.ImageUrl,
                SellerName = auction.Seller.Name,
                CategoryName = auction.Category.Name,
                Status = auction.Status,
                BasePrice = auction.BasePrice,
                MinimumIncrement = auction.MinimumIncrement,
                HighestBid = auction.Bids.Max(bid => (decimal?)bid.Amount),
                StartAtUtc = DateTime.SpecifyKind(
                auction.StartAtUtc,
                DateTimeKind.Utc),

                EndAtUtc = DateTime.SpecifyKind(
                            auction.EndAtUtc,
                            DateTimeKind.Utc),
                Bids = auction.Bids
                    .OrderByDescending(bid => bid.BidAtUtc)
                    .Select(bid => new BidHistoryItemDto
                    {
                        Id = bid.Id,
                        BuyerAlias = "Comprador #" + bid.BuyerId,
                        Amount = bid.Amount,                        BidAtUtc = bid.BidAtUtc
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> SellerExistsAsync(int sellerId)
    {
        return await _context.Users.AnyAsync(user => user.Id == sellerId);
    }

    public async Task<bool> CategoryExistsAsync(int categoryId)
    {
        return await _context.Categories.AnyAsync(category =>
            category.Id == categoryId);
    }

    public async Task<Auction> CreateAsync(Auction auction)
    {
        _context.Auctions.Add(auction);

        await _context.SaveChangesAsync();

        return auction;
    }
}