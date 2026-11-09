using CampusPlacementSystem.Data;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.CompanyHR;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementSystem.Services.Implementations;

public class CompanyHRService : ICompanyHRService
{
    private readonly ICompanyHRRepository _companyHRRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IAuthRepository _authRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;
    private readonly ILogger<CompanyHRService> _logger;

    public CompanyHRService(
        ICompanyHRRepository companyHRRepository,
        ICompanyRepository companyRepository,
        IAuthRepository authRepository,
        UserManager<ApplicationUser> userManager,
        AppDbContext context,
        ILogger<CompanyHRService> logger)
    {
        _companyHRRepository = companyHRRepository;
        _companyRepository = companyRepository;
        _authRepository = authRepository;
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<CompanyHRResponseDto>> RegisterCompanyHRAsync(RegisterCompanyHRRequestDto request)
    {
        var company = await _companyRepository.GetByIdAsync(request.CompanyId);
        if (company == null)
        {
            return ApiResponse<CompanyHRResponseDto>.FailureResult("The specified company was not found.");
        }

        if (company.Status != CompanyStatus.Approved)
        {
            return ApiResponse<CompanyHRResponseDto>.FailureResult("Company HR registration is only permitted for approved and active companies.");
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return ApiResponse<CompanyHRResponseDto>.FailureResult("A user with this email address is already registered.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var user = new ApplicationUser
            {
                UserName = request.Email.Trim().ToLower(),
                Email = request.Email.Trim().ToLower(),
                FullName = request.FullName.Trim(),
                PhoneNumber = request.PhoneNumber?.Trim(),
                AccountStatus = AccountStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var userResult = await _userManager.CreateAsync(user, request.Password);
            if (!userResult.Succeeded)
            {
                var errors = userResult.Errors.Select(e => e.Description).ToList();
                return ApiResponse<CompanyHRResponseDto>.FailureResult("Failed to create user account.", errors);
            }

            var companyHR = new CompanyHR
            {
                UserId = user.Id,
                CompanyId = request.CompanyId,
                Designation = request.Designation.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _companyHRRepository.AddAsync(companyHR);
            await transaction.CommitAsync();

            _logger.LogInformation("Company HR registered successfully with HRId: {HRId}, UserId: {UserId}", companyHR.CompanyHRId, user.Id);

            var response = MapToResponseDto(companyHR, user, company);
            return ApiResponse<CompanyHRResponseDto>.SuccessResult(response, "Company HR registered successfully. Awaiting SuperAdmin approval.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Transaction failed while registering company HR.");
            throw;
        }
    }

    public async Task<ApiResponse<PagedResult<CompanyHRResponseDto>>> GetAllCompanyHRsAsync(PaginationParams paginationParams, AccountStatus? status = null, int? companyId = null)
    {
        var result = await _companyHRRepository.GetAllAsync(paginationParams, status, companyId);
        return ApiResponse<PagedResult<CompanyHRResponseDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<PagedResult<CompanyHRResponseDto>>> GetPendingCompanyHRsAsync(PaginationParams paginationParams)
    {
        var result = await _companyHRRepository.GetAllAsync(paginationParams, AccountStatus.Pending);
        return ApiResponse<PagedResult<CompanyHRResponseDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<CompanyHRResponseDto>> GetCompanyHRByIdAsync(int id)
    {
        var hr = await _companyHRRepository.GetByIdAsync(id);
        if (hr == null)
        {
            return ApiResponse<CompanyHRResponseDto>.FailureResult("Company HR profile not found.");
        }

        return ApiResponse<CompanyHRResponseDto>.SuccessResult(MapToResponseDto(hr, hr.User, hr.Company));
    }

    public async Task<ApiResponse<CompanyHRResponseDto>> GetCurrentHRProfileAsync(string userId)
    {
        var hr = await _companyHRRepository.GetByUserIdAsync(userId);
        if (hr == null)
        {
            return ApiResponse<CompanyHRResponseDto>.FailureResult("Company HR profile not found.");
        }

        return ApiResponse<CompanyHRResponseDto>.SuccessResult(MapToResponseDto(hr, hr.User, hr.Company));
    }

    public async Task<ApiResponse<CompanyHRResponseDto>> UpdateCurrentHRProfileAsync(string userId, UpdateCompanyHRProfileRequestDto request)
    {
        var hr = await _companyHRRepository.GetByUserIdAsync(userId);
        if (hr == null)
        {
            return ApiResponse<CompanyHRResponseDto>.FailureResult("Company HR profile not found.");
        }

        hr.Designation = request.Designation.Trim();
        hr.User.FullName = request.FullName.Trim();
        hr.User.PhoneNumber = request.PhoneNumber?.Trim();
        hr.User.UpdatedAt = DateTime.UtcNow;

        await _companyHRRepository.UpdateAsync(hr);
        _logger.LogInformation("Company HR profile updated for UserId: {UserId}", userId);

        return ApiResponse<CompanyHRResponseDto>.SuccessResult(MapToResponseDto(hr, hr.User, hr.Company), "Profile updated successfully.");
    }

    public async Task<ApiResponse<bool>> ApproveCompanyHRAsync(int id, string approvedByUserId)
    {
        var hr = await _companyHRRepository.GetByIdAsync(id);
        if (hr == null)
        {
            return ApiResponse<bool>.FailureResult("Company HR account not found.");
        }

        if (hr.User.AccountStatus == AccountStatus.Active)
        {
            return ApiResponse<bool>.FailureResult("Company HR account is already active.");
        }

        hr.ApprovedBy = approvedByUserId;
        hr.ApprovedAt = DateTime.UtcNow;
        hr.User.AccountStatus = AccountStatus.Active;
        hr.User.UpdatedAt = DateTime.UtcNow;

        if (!await _userManager.IsInRoleAsync(hr.User, AppRoles.CompanyHR))
        {
            await _userManager.AddToRoleAsync(hr.User, AppRoles.CompanyHR);
        }

        await _companyHRRepository.UpdateAsync(hr);
        _logger.LogInformation("Company HR ID {HRId} approved by {AdminId}", id, approvedByUserId);

        return ApiResponse<bool>.SuccessResult(true, "Company HR approved successfully. Login access is now enabled.");
    }

    public async Task<ApiResponse<bool>> RejectCompanyHRAsync(int id)
    {
        var hr = await _companyHRRepository.GetByIdAsync(id);
        if (hr == null)
        {
            return ApiResponse<bool>.FailureResult("Company HR account not found.");
        }

        hr.User.AccountStatus = AccountStatus.Rejected;
        hr.User.UpdatedAt = DateTime.UtcNow;

        await _companyHRRepository.UpdateAsync(hr);
        _logger.LogInformation("Company HR ID {HRId} rejected", id);

        return ApiResponse<bool>.SuccessResult(true, "Company HR rejected successfully.");
    }

    public async Task<ApiResponse<bool>> SuspendCompanyHRAsync(int id)
    {
        var hr = await _companyHRRepository.GetByIdAsync(id);
        if (hr == null)
        {
            return ApiResponse<bool>.FailureResult("Company HR account not found.");
        }

        hr.User.AccountStatus = AccountStatus.Suspended;
        hr.User.UpdatedAt = DateTime.UtcNow;

        await _companyHRRepository.UpdateAsync(hr);
        await _authRepository.RevokeUserRefreshTokensAsync(hr.UserId);

        _logger.LogInformation("Company HR ID {HRId} suspended", id);
        return ApiResponse<bool>.SuccessResult(true, "Company HR suspended successfully.");
    }

    private static CompanyHRResponseDto MapToResponseDto(CompanyHR hr, ApplicationUser u, Company c)
    {
        return new CompanyHRResponseDto
        {
            CompanyHRId = hr.CompanyHRId,
            UserId = hr.UserId,
            FullName = u.FullName,
            Email = u.Email ?? string.Empty,
            PhoneNumber = u.PhoneNumber,
            CompanyId = hr.CompanyId,
            CompanyName = c.CompanyName,
            Designation = hr.Designation,
            AccountStatus = u.AccountStatus,
            ApprovedBy = hr.ApprovedBy,
            ApprovedAt = hr.ApprovedAt,
            CreatedAt = hr.CreatedAt,
            TotalJobsPosted = hr.CreatedJobOpenings?.Count ?? 0
        };
    }
}
