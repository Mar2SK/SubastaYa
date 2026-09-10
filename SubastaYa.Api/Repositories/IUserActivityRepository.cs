using SubastaYa.Api.Dtos.Users;

namespace SubastaYa.Api.Repositories;

public interface IUserActivityRepository
{
    Task<bool> UserExistsAsync(int userId);

    Task<List<UserBidActivityDto>> GetBidActivitiesAsync(int userId);

    Task<List<UserPublicationActivityDto>> GetPublicationActivitiesAsync(
        int userId);
}