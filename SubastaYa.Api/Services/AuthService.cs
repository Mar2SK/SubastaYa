using Microsoft.EntityFrameworkCore;
using SubastaYa.Api.Data;
using SubastaYa.Api.Dtos.Auth;
using SubastaYa.Api.Models;

namespace SubastaYa.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LoginResponseDto?> LoginAsync(
        LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        string email = request.Email.Trim();

        User? user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(currentUser =>
                currentUser.Email == email);

        if (user is null ||
            user.PasswordHash != request.Password)
        {
            return null;
        }

        return MapUser(user);
    }

    public async Task<LoginResponseDto?> RegisterAsync(
        RegisterRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        string email = request.Email.Trim();

        bool emailExists = await _context.Users
            .AnyAsync(currentUser =>
                currentUser.Email == email);

        if (emailExists)
        {
            return null;
        }

        User user = new()
        {
            Email = email,
            Name = request.Name.Trim(),
            PasswordHash = request.Password,
            RegisteredAtUtc = DateTime.UtcNow
        };

        Wallet wallet = new()
        {
            User = user,
            TotalBalance = 0,
            HeldBalance = 0,
            AvailableBalance = 0,
            Version = 1
        };

        user.Wallet = wallet;

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return MapUser(user);
    }

    private static LoginResponseDto MapUser(User user)
    {
        return new LoginResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name
        };
    }
}