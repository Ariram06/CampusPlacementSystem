using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CampusPlacementSystem.DTOs.Auth;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace CampusPlacementSystem.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IAuthRepository authRepository, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _authRepository = authRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request)
    {
        var user = await _authRepository.GetUserByEmailAsync(request.Email);
        if (user == null)
        {
            return ApiResponse<LoginResponseDto>.FailureResult("Invalid email or password.");
        }

        var isPasswordValid = await _authRepository.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
        {
            return ApiResponse<LoginResponseDto>.FailureResult("Invalid email or password.");
        }

        // Account status validation
        switch (user.AccountStatus)
        {
            case AccountStatus.Pending:
                return ApiResponse<LoginResponseDto>.FailureResult("Your account is pending approval by the administrator.");
            case AccountStatus.Rejected:
                return ApiResponse<LoginResponseDto>.FailureResult("Your account has been rejected.");
            case AccountStatus.Suspended:
                return ApiResponse<LoginResponseDto>.FailureResult("Your account has been suspended. Please contact support.");
            case AccountStatus.Inactive:
                return ApiResponse<LoginResponseDto>.FailureResult("Your account is inactive.");
            case AccountStatus.Active:
                break;
            default:
                return ApiResponse<LoginResponseDto>.FailureResult("Account status does not permit login.");
        }

        var roles = await _authRepository.GetUserRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? string.Empty;

        var tokenExpiry = DateTime.UtcNow.AddMinutes(GetTokenValidityInMinutes());
        var accessToken = GenerateJwtToken(user, roles, tokenExpiry);

        var rawRefreshToken = GenerateSecureRefreshToken();
        var tokenHash = HashToken(rawRefreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenValidityInDays()),
            CreatedAt = DateTime.UtcNow
        };

        await _authRepository.AddRefreshTokenAsync(refreshTokenEntity);

        var (profileId, collegeId, _, companyId, _) = await _authRepository.GetUserAssociationsAsync(user.Id, primaryRole);

        var response = new LoginResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = primaryRole,
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            ExpiresAt = tokenExpiry,
            ProfileId = profileId,
            AssociatedCollegeId = collegeId,
            AssociatedCompanyId = companyId
        };

        return ApiResponse<LoginResponseDto>.SuccessResult(response, "Login successful.");
    }

    public async Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var existingToken = await _authRepository.GetRefreshTokenByHashAsync(tokenHash);

        if (existingToken == null)
        {
            return ApiResponse<TokenResponseDto>.FailureResult("Invalid refresh token.");
        }

        // Token reuse detection: if revoked token is used again, revoke all tokens for this user
        if (existingToken.IsRevoked)
        {
            _logger.LogWarning("Potential refresh token reuse attack detected for user {UserId}", existingToken.UserId);
            await _authRepository.RevokeUserRefreshTokensAsync(existingToken.UserId);
            return ApiResponse<TokenResponseDto>.FailureResult("Refresh token has been revoked. All sessions invalidated for security.");
        }

        if (existingToken.IsExpired)
        {
            return ApiResponse<TokenResponseDto>.FailureResult("Refresh token has expired. Please log in again.");
        }

        var user = existingToken.User;
        if (user.AccountStatus != AccountStatus.Active)
        {
            return ApiResponse<TokenResponseDto>.FailureResult("Account is not active. Refresh token denied.");
        }

        var roles = await _authRepository.GetUserRolesAsync(user);

        // Rotate Refresh Token
        var newRawRefreshToken = GenerateSecureRefreshToken();
        var newTokenHash = HashToken(newRawRefreshToken);

        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.ReplacedByTokenHash = newTokenHash;
        await _authRepository.UpdateRefreshTokenAsync(existingToken);

        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = newTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(GetRefreshTokenValidityInDays()),
            CreatedAt = DateTime.UtcNow
        };
        await _authRepository.AddRefreshTokenAsync(newRefreshTokenEntity);

        var tokenExpiry = DateTime.UtcNow.AddMinutes(GetTokenValidityInMinutes());
        var newAccessToken = GenerateJwtToken(user, roles, tokenExpiry);

        var response = new TokenResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRawRefreshToken,
            ExpiresAt = tokenExpiry
        };

        return ApiResponse<TokenResponseDto>.SuccessResult(response, "Tokens refreshed successfully.");
    }

    public async Task<ApiResponse<bool>> RevokeTokenAsync(RevokeTokenRequestDto request, string? requestingUserId = null)
    {
        var tokenHash = HashToken(request.RefreshToken);
        var token = await _authRepository.GetRefreshTokenByHashAsync(tokenHash);

        if (token == null)
        {
            return ApiResponse<bool>.FailureResult("Refresh token not found.");
        }

        if (requestingUserId != null && token.UserId != requestingUserId)
        {
            return ApiResponse<bool>.FailureResult("You are not authorized to revoke this token.");
        }

        if (!token.IsRevoked)
        {
            token.RevokedAt = DateTime.UtcNow;
            await _authRepository.UpdateRefreshTokenAsync(token);
        }

        return ApiResponse<bool>.SuccessResult(true, "Refresh token revoked successfully.");
    }

    public async Task<ApiResponse<bool>> ChangePasswordAsync(string userId, ChangePasswordRequestDto request)
    {
        var user = await _authRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<bool>.FailureResult("User not found.");
        }

        var result = await _authRepository.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse<bool>.FailureResult("Password change failed.", errors);
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _authRepository.SaveChangesAsync();

        // Revoke all existing refresh tokens after password change for security
        await _authRepository.RevokeUserRefreshTokensAsync(userId);

        return ApiResponse<bool>.SuccessResult(true, "Password changed successfully. Please log in again.");
    }

    public async Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request)
    {
        var user = await _authRepository.GetUserByEmailAsync(request.Email);
        if (user == null || user.AccountStatus != AccountStatus.Active)
        {
            // Do not reveal user non-existence for security
            return ApiResponse<string>.SuccessResult(string.Empty, "If an active account exists with this email, a password reset token has been generated.");
        }

        var resetToken = await _authRepository.GeneratePasswordResetTokenAsync(user);

        return ApiResponse<string>.SuccessResult(resetToken, "Password reset token generated successfully.");
    }

    public async Task<ApiResponse<bool>> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        var user = await _authRepository.GetUserByEmailAsync(request.Email);
        if (user == null)
        {
            return ApiResponse<bool>.FailureResult("Invalid password reset request.");
        }

        var result = await _authRepository.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ApiResponse<bool>.FailureResult("Password reset failed.", errors);
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _authRepository.SaveChangesAsync();

        // Invalidate active refresh tokens
        await _authRepository.RevokeUserRefreshTokensAsync(user.Id);

        return ApiResponse<bool>.SuccessResult(true, "Password reset successfully. You can now log in with your new password.");
    }

    public async Task<ApiResponse<CurrentUserResponseDto>> GetCurrentUserAsync(string userId)
    {
        var user = await _authRepository.GetUserByIdAsync(userId);
        if (user == null)
        {
            return ApiResponse<CurrentUserResponseDto>.FailureResult("User not found.");
        }

        var roles = await _authRepository.GetUserRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? string.Empty;

        var (profileId, collegeId, collegeName, companyId, companyName) = await _authRepository.GetUserAssociationsAsync(user.Id, primaryRole);

        var response = new CurrentUserResponseDto
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = primaryRole,
            AccountStatus = user.AccountStatus,
            CreatedAt = user.CreatedAt,
            ProfileId = profileId,
            AssociatedCollegeId = collegeId,
            AssociatedCollegeName = collegeName,
            AssociatedCompanyId = companyId,
            AssociatedCompanyName = companyName
        };

        return ApiResponse<CurrentUserResponseDto>.SuccessResult(response);
    }

    // Helper methods
    private string GenerateJwtToken(ApplicationUser user, IList<string> roles, DateTime expiresAt)
    {
        var jwtConfig = _configuration.GetSection("JwtConfig");
        var secretKey = jwtConfig["SecretKey"] ?? throw new InvalidOperationException("SecretKey is not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: jwtConfig["Issuer"],
            audience: jwtConfig["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateSecureRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }

    private int GetTokenValidityInMinutes()
    {
        return int.TryParse(_configuration["JwtConfig:TokenValidityInMinutes"], out var minutes) ? minutes : 60;
    }

    private int GetRefreshTokenValidityInDays()
    {
        return int.TryParse(_configuration["JwtConfig:RefreshTokenValidityInDays"], out var days) ? days : 7;
    }
}
