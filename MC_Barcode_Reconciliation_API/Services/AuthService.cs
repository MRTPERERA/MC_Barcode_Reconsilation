using MC_Barcode_Reconciliation_API.Data;
using MC_Barcode_Reconciliation_API.DTOs;
using Microsoft.EntityFrameworkCore;

namespace MC_Barcode_Reconciliation_API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        if (string.IsNullOrWhiteSpace(request?.UserId) || string.IsNullOrWhiteSpace(request?.Password))
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "UserId and Password are required."
            };
        }

        var user = await _context.UserAccounts
            .FirstOrDefaultAsync(u => u.UserId == request.UserId);

        if (user is null)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = $"User '{request.UserId}' not found."
            };
        }

        // Simple password validation (case-sensitive string comparison)
        // Note: In production, use proper password hashing (bcrypt, PBKDF2, etc.)
        if (user.Password != request.Password)
        {
            return new LoginResponseDto
            {
                Success = false,
                Message = "Invalid password."
            };
        }

        return new LoginResponseDto
        {
            Success = true,
            Message = "Login successful.",
            User = MapToUserInfoDto(user)
        };
    }

    public async Task<UserInfoDto?> GetUserByIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return null;

        var user = await _context.UserAccounts
            .FirstOrDefaultAsync(u => u.UserId == userId);

        return user is null ? null : MapToUserInfoDto(user);
    }

    public async Task<bool> ValidateUserAsync(string userId, string password)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(password))
            return false;

        var user = await _context.UserAccounts
            .FirstOrDefaultAsync(u => u.UserId == userId && u.Password == password);

        return user is not null;
    }

    private static UserInfoDto MapToUserInfoDto(Models.UserAccount user) => new()
    {
        UserId = user.UserId,
        Level = user.Level,
        Site = user.Site,
        SystemName = user.SystemName,
        DefaultLocation = user.DefaultLocation,
        PlantLoc = user.PlantLoc,
        IfsSite = user.IfsSite,
        IfsSiteName = user.IfsSiteName,
        RecevingCat = user.RecevingCat
    };
}
