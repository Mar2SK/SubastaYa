using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Dtos.Users;
using SubastaYa.Api.Services;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly IUserActivityService _userActivityService;

    public UsersController(IUserActivityService userActivityService)
    {
        _userActivityService = userActivityService;
    }

    [HttpGet("{userId:int}/bids")]
    public async Task<ActionResult<List<UserBidActivityDto>>> GetBids(
        int userId)
    {
        List<UserBidActivityDto> activities =
            await _userActivityService.GetBidActivitiesAsync(userId);

        return Ok(activities);
    }

    [HttpGet("{userId:int}/auctions")]
    public async Task<ActionResult<List<UserPublicationActivityDto>>>
        GetAuctions(int userId)
    {
        List<UserPublicationActivityDto> activities =
            await _userActivityService
                .GetPublicationActivitiesAsync(userId);

        return Ok(activities);
    }
}