using MC_Barcode_Reconciliation_API.DTOs;

namespace MC_Barcode_Reconciliation_API.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
    Task<UserInfoDto?> GetUserByIdAsync(string userId);
    Task<bool> ValidateUserAsync(string userId, string password);
}
