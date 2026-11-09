using CampusPlacementSystem.Data;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.PlacementOfficer;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementSystem.Services.Implementations;

public class PlacementOfficerService : IPlacementOfficerService
{
    private readonly IPlacementOfficerRepository _officerRepository;
    private readonly ICollegeRepository _collegeRepository;
    private readonly IAuthRepository _authRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _context;
    private readonly ILogger<PlacementOfficerService> _logger;

    public PlacementOfficerService(
        IPlacementOfficerRepository officerRepository,
        ICollegeRepository collegeRepository,
        IAuthRepository authRepository,
        UserManager<ApplicationUser> userManager,
        AppDbContext context,
        ILogger<PlacementOfficerService> logger)
    {
        _officerRepository = officerRepository;
        _collegeRepository = collegeRepository;
        _authRepository = authRepository;
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<OfficerResponseDto>> RegisterOfficerAsync(RegisterOfficerRequestDto request)
    {
        var college = await _collegeRepository.GetByIdAsync(request.CollegeId);
        if (college == null)
        {
            return ApiResponse<OfficerResponseDto>.FailureResult("The specified college was not found.");
        }

        if (college.Status != CollegeStatus.Approved)
        {
            return ApiResponse<OfficerResponseDto>.FailureResult("Placement officer registration is only permitted for approved and active colleges.");
        }

        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return ApiResponse<OfficerResponseDto>.FailureResult("A user with this email address is already registered.");
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
                return ApiResponse<OfficerResponseDto>.FailureResult("Failed to create user account.", errors);
            }

            var officer = new PlacementOfficer
            {
                UserId = user.Id,
                CollegeId = request.CollegeId,
                Designation = request.Designation.Trim(),
                Status = OfficerStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            await _officerRepository.AddAsync(officer);
            await transaction.CommitAsync();

            _logger.LogInformation("Placement officer registered successfully with OfficerId: {OfficerId}, UserId: {UserId}", officer.PlacementOfficerId, user.Id);

            var response = MapToResponseDto(officer, user, college);
            return ApiResponse<OfficerResponseDto>.SuccessResult(response, "Placement officer registered successfully. Awaiting SuperAdmin approval.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Transaction failed while registering placement officer.");
            throw;
        }
    }

    public async Task<ApiResponse<PagedResult<OfficerResponseDto>>> GetAllOfficersAsync(PaginationParams paginationParams, OfficerStatus? status = null, int? collegeId = null)
    {
        var result = await _officerRepository.GetAllAsync(paginationParams, status, collegeId);
        return ApiResponse<PagedResult<OfficerResponseDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<PagedResult<OfficerResponseDto>>> GetPendingOfficersAsync(PaginationParams paginationParams)
    {
        var result = await _officerRepository.GetAllAsync(paginationParams, OfficerStatus.Pending);
        return ApiResponse<PagedResult<OfficerResponseDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<OfficerResponseDto>> GetOfficerByIdAsync(int id)
    {
        var officer = await _officerRepository.GetByIdAsync(id);
        if (officer == null)
        {
            return ApiResponse<OfficerResponseDto>.FailureResult("Placement officer not found.");
        }

        return ApiResponse<OfficerResponseDto>.SuccessResult(MapToResponseDto(officer, officer.User, officer.College));
    }

    public async Task<ApiResponse<OfficerResponseDto>> GetCurrentOfficerProfileAsync(string userId)
    {
        var officer = await _officerRepository.GetByUserIdAsync(userId);
        if (officer == null)
        {
            return ApiResponse<OfficerResponseDto>.FailureResult("Placement officer profile not found.");
        }

        return ApiResponse<OfficerResponseDto>.SuccessResult(MapToResponseDto(officer, officer.User, officer.College));
    }

    public async Task<ApiResponse<OfficerResponseDto>> UpdateOfficerProfileAsync(string userId, UpdateOfficerProfileRequestDto request)
    {
        var officer = await _officerRepository.GetByUserIdAsync(userId);
        if (officer == null)
        {
            return ApiResponse<OfficerResponseDto>.FailureResult("Placement officer profile not found.");
        }

        officer.Designation = request.Designation.Trim();
        officer.User.FullName = request.FullName.Trim();
        officer.User.PhoneNumber = request.PhoneNumber?.Trim();
        officer.User.UpdatedAt = DateTime.UtcNow;

        await _officerRepository.UpdateAsync(officer);
        _logger.LogInformation("Officer profile updated for UserId: {UserId}", userId);

        return ApiResponse<OfficerResponseDto>.SuccessResult(MapToResponseDto(officer, officer.User, officer.College), "Profile updated successfully.");
    }

    public async Task<ApiResponse<bool>> ApproveOfficerAsync(int id, string approvedByUserId)
    {
        var officer = await _officerRepository.GetByIdAsync(id);
        if (officer == null)
        {
            return ApiResponse<bool>.FailureResult("Placement officer not found.");
        }

        if (officer.Status == OfficerStatus.Approved)
        {
            return ApiResponse<bool>.FailureResult("Placement officer is already approved.");
        }

        officer.Status = OfficerStatus.Approved;
        officer.ApprovedBy = approvedByUserId;
        officer.ApprovedAt = DateTime.UtcNow;

        // Activate User Account and assign role
        officer.User.AccountStatus = AccountStatus.Active;
        officer.User.UpdatedAt = DateTime.UtcNow;

        if (!await _userManager.IsInRoleAsync(officer.User, AppRoles.PlacementOfficer))
        {
            await _userManager.AddToRoleAsync(officer.User, AppRoles.PlacementOfficer);
        }

        await _officerRepository.UpdateAsync(officer);
        _logger.LogInformation("Placement officer ID {OfficerId} approved by {AdminId}", id, approvedByUserId);

        return ApiResponse<bool>.SuccessResult(true, "Placement officer approved successfully. Login access is now enabled.");
    }

    public async Task<ApiResponse<bool>> RejectOfficerAsync(int id)
    {
        var officer = await _officerRepository.GetByIdAsync(id);
        if (officer == null)
        {
            return ApiResponse<bool>.FailureResult("Placement officer not found.");
        }

        officer.Status = OfficerStatus.Rejected;
        officer.User.AccountStatus = AccountStatus.Rejected;
        officer.User.UpdatedAt = DateTime.UtcNow;

        await _officerRepository.UpdateAsync(officer);
        _logger.LogInformation("Placement officer ID {OfficerId} rejected", id);

        return ApiResponse<bool>.SuccessResult(true, "Placement officer rejected successfully.");
    }

    public async Task<ApiResponse<bool>> SuspendOfficerAsync(int id)
    {
        var officer = await _officerRepository.GetByIdAsync(id);
        if (officer == null)
        {
            return ApiResponse<bool>.FailureResult("Placement officer not found.");
        }

        officer.Status = OfficerStatus.Suspended;
        officer.User.AccountStatus = AccountStatus.Suspended;
        officer.User.UpdatedAt = DateTime.UtcNow;

        await _officerRepository.UpdateAsync(officer);
        await _authRepository.RevokeUserRefreshTokensAsync(officer.UserId);

        _logger.LogInformation("Placement officer ID {OfficerId} suspended", id);
        return ApiResponse<bool>.SuccessResult(true, "Placement officer suspended successfully.");
    }

    private static OfficerResponseDto MapToResponseDto(PlacementOfficer po, ApplicationUser u, College c)
    {
        return new OfficerResponseDto
        {
            PlacementOfficerId = po.PlacementOfficerId,
            UserId = po.UserId,
            FullName = u.FullName,
            Email = u.Email ?? string.Empty,
            PhoneNumber = u.PhoneNumber,
            CollegeId = po.CollegeId,
            CollegeName = c.CollegeName,
            CollegeCode = c.CollegeCode,
            Designation = po.Designation,
            Status = po.Status,
            ApprovedBy = po.ApprovedBy,
            ApprovedAt = po.ApprovedAt,
            CreatedAt = po.CreatedAt,
            TotalEnrolledStudents = po.EnrolledStudents?.Count ?? 0
        };
    }
}
