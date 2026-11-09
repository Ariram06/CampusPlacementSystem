using CampusPlacementSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementSystem.Repositories.Interfaces;

public interface IAuthRepository
{
    Task<ApplicationUser?> GetUserByEmailAsync(string email);
    Task<ApplicationUser?> GetUserByIdAsync(string userId);
    Task<IList<string>> GetUserRolesAsync(ApplicationUser user);
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
    Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword);
    Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user);
    Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword);
    Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash);
    Task AddRefreshTokenAsync(RefreshToken refreshToken);
    Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
    Task RevokeUserRefreshTokensAsync(string userId, string? replacedByHash = null);
    Task<(int? profileId, int? collegeId, string? collegeName, int? companyId, string? companyName)> GetUserAssociationsAsync(string userId, string role);
    Task SaveChangesAsync();
}
