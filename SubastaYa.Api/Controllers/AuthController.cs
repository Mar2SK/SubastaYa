using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Dtos.Auth;
using SubastaYa.Api.Services;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(
        [FromBody] LoginRequestDto request)
    {
        LoginResponseDto? result =
            await _authService.LoginAsync(request);

        if (result is null)
        {
            return Unauthorized(new
            {
                message =
                    "[CODE-ERROR] - Email o contraseña incorrectos."
            });
        }

        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponseDto>> Register(
        [FromBody] RegisterRequestDto request)
    {
        LoginResponseDto? result =
            await _authService.RegisterAsync(request);

        if (result is null)
        {
            return BadRequest(new
            {
                message =
                    "[CODE-ERROR] - Los datos son inválidos o el email ya está registrado."
            });
        }

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }
}