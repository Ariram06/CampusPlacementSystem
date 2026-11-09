using CampusPlacementSystem.DTOs.Auth;
using CampusPlacementSystem.DTOs.Common;

namespace CampusPlacementSystem.Services.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request);
    Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request);
    Task<ApiResponse<bool>> RevokeTokenAsync(RevokeTokenRequestDto request, string? requestingUserId = null);
    Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordRequestDto request);
    Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request);
    Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequestDto request);
    Task<ApiResponse<CurrentUserResponseDto>> GetCurrentUserAsync(string userId);
}
