using Microsoft.AspNetCore.Http;
using SubastaYa.Api.Dtos.Users;
using SubastaYa.Api.Helpers;
using SubastaYa.Api.Repositories;

namespace SubastaYa.Api.Services;

public class UserActivityService : IUserActivityService
{
    private readonly IUserActivityRepository _userActivityRepository;

    public UserActivityService(
        IUserActivityRepository userActivityRepository)
    {
        _userActivityRepository = userActivityRepository;
    }

    public async Task<List<UserBidActivityDto>> GetBidActivitiesAsync(
        int userId)
    {
        await EnsureUserExistsAsync(userId);

        return await _userActivityRepository.GetBidActivitiesAsync(userId);
    }

    public async Task<List<UserPublicationActivityDto>>
        GetPublicationActivitiesAsync(int userId)
    {
        await EnsureUserExistsAsync(userId);

        return await _userActivityRepository
            .GetPublicationActivitiesAsync(userId);
    }

    private async Task EnsureUserExistsAsync(int userId)
    {
        bool userExists =
            await _userActivityRepository.UserExistsAsync(userId);

        if (!userExists)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "el usuario indicado no existe.");
        }
    }
}