using CampusPlacementSystem.Data;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementSystem.Repositories.Implementations;

public class AuthRepository : IAuthRepository
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;

    public AuthRepository(UserManager<ApplicationUser> userManager, AppDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId);
    }

    public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<IdentityResult> ChangePasswordAsync(ApplicationUser user, string currentPassword, string newPassword)
    {
        return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<IdentityResult> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
    {
        return await _userManager.ResetPasswordAsync(user, token, newPassword);
    }

    public async Task<RefreshToken?> GetRefreshTokenByHashAsync(string tokenHash)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
    }

    public async Task AddRefreshTokenAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeUserRefreshTokensAsync(string userId, string? replacedByHash = null)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(replacedByHash))
            {
                token.ReplacedByTokenHash = replacedByHash;
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<(int? profileId, int? collegeId, string? collegeName, int? companyId, string? companyName)> GetUserAssociationsAsync(string userId, string role)
    {
        int? profileId = null;
        int? collegeId = null;
        string? collegeName = null;
        int? companyId = null;
        string? companyName = null;

        switch (role)
        {
            case AppRoles.PlacementOfficer:
                var officer = await _context.PlacementOfficers
                    .Include(po => po.College)
                    .FirstOrDefaultAsync(po => po.UserId == userId);
                if (officer != null)
                {
                    profileId = officer.PlacementOfficerId;
                    collegeId = officer.CollegeId;
                    collegeName = officer.College?.CollegeName;
                }
                break;

            case AppRoles.CompanyHR:
                var hr = await _context.CompanyHRs
                    .Include(h => h.Company)
                    .FirstOrDefaultAsync(h => h.UserId == userId);
                if (hr != null)
                {
                    profileId = hr.CompanyHRId;
                    companyId = hr.CompanyId;
                    companyName = hr.Company?.CompanyName;
                }
                break;

            case AppRoles.Student:
                var student = await _context.Students
                    .Include(s => s.College)
                    .FirstOrDefaultAsync(s => s.UserId == userId);
                if (student != null)
                {
                    profileId = student.StudentId;
                    collegeId = student.CollegeId;
                    collegeName = student.College?.CollegeName;
                }
                break;
        }

        return (profileId, collegeId, collegeName, companyId, companyName);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
