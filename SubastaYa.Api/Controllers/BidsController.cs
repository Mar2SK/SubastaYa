using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Dtos.Bids;
using SubastaYa.Api.Services;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/v1/auctions/{auctionId:int}/bids")]
public class BidsController : ControllerBase
{
    private readonly IBidService _bidService;

    public BidsController(IBidService bidService)
    {
        _bidService = bidService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(BidResultDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BidResultDto>> Create(
        int auctionId,
        [FromBody] CreateBidRequestDto request)
    {
        BidResultDto result = await _bidService.CreateAsync(
            auctionId,
            request);

        return StatusCode(StatusCodes.Status201Created, result);
    }
}