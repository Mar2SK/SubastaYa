using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Dtos;
using SubastaYa.Api.Dtos.Auctions;
using SubastaYa.Api.Services;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/v1/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly IAuctionService _auctionService;

    public AuctionsController(IAuctionService auctionService)
    {
        _auctionService = auctionService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResponseDto<AuctionListItemDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponseDto<AuctionListItemDto>>> GetAll(
        [FromQuery] AuctionQueryDto query)
    {
        PagedResponseDto<AuctionListItemDto> result =
            await _auctionService.GetAllAsync(query);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AuctionDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuctionDetailDto>> GetById(int id)
    {
        AuctionDetailDto? auction = await _auctionService.GetByIdAsync(id);

        if (auction is null)
        {
            return NotFound(new
            {
                message = "[CODE-ERROR] - la subasta solicitada no existe."
            });
        }

        return Ok(auction);
    }

    [HttpPost]
    [ProducesResponseType(typeof(AuctionDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuctionDetailDto>> Create(
    [FromBody] CreateAuctionRequestDto request)
    {
        AuctionDetailDto createdAuction =
            await _auctionService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdAuction.Id },
            createdAuction);
    }
}