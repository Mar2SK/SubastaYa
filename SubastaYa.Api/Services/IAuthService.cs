using SubastaYa.Api.Dtos.Auth;

namespace SubastaYa.Api.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request);

    Task<LoginResponseDto?> RegisterAsync(RegisterRequestDto request);
}