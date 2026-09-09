using SubastaYa.Api.Dtos.Users;

namespace SubastaYa.Api.Services;

public interface IUserActivityService
{
    Task<List<UserBidActivityDto>> GetBidActivitiesAsync(int userId);

    Task<List<UserPublicationActivityDto>> GetPublicationActivitiesAsync(
        int userId);
}