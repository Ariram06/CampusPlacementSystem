using CampusPlacementSystem.Data;
using CampusPlacementSystem.DTOs.College;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementSystem.Services.Implementations;

public class CollegeService : ICollegeService
{
    private readonly ICollegeRepository _collegeRepository;
    private readonly AppDbContext _context;
    private readonly ILogger<CollegeService> _logger;

    public CollegeService(ICollegeRepository collegeRepository, AppDbContext context, ILogger<CollegeService> logger)
    {
        _collegeRepository = collegeRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<CollegeResponseDto>> RegisterCollegeAsync(CreateCollegeRequestDto request)
    {
        var codeExists = await _collegeRepository.CollegeCodeExistsAsync(request.CollegeCode);
        if (codeExists)
        {
            return ApiResponse<CollegeResponseDto>.FailureResult("A college with this code is already registered.");
        }

        var emailExists = await _collegeRepository.OfficialEmailExistsAsync(request.OfficialEmail);
        if (emailExists)
        {
            return ApiResponse<CollegeResponseDto>.FailureResult("A college with this official email is already registered.");
        }

        var college = new College
        {
            CollegeName = request.CollegeName.Trim(),
            CollegeCode = request.CollegeCode.Trim().ToUpper(),
            UniversityName = request.UniversityName.Trim(),
            OfficialEmail = request.OfficialEmail.Trim().ToLower(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Address = request.Address.Trim(),
            Website = request.Website?.Trim(),
            Status = CollegeStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _collegeRepository.AddAsync(college);
        _logger.LogInformation("College registered successfully with ID: {CollegeId}", college.CollegeId);

        var response = MapToResponseDto(college);
        return ApiResponse<CollegeResponseDto>.SuccessResult(response, "College registered successfully. Awaiting SuperAdmin approval.");
    }

    public async Task<ApiResponse<PagedResult<CollegeResponseDto>>> GetAllCollegesAsync(PaginationParams paginationParams, CollegeStatus? status = null)
    {
        var pagedResult = await _collegeRepository.GetAllAsync(paginationParams, status);
        return ApiResponse<PagedResult<CollegeResponseDto>>.SuccessResult(pagedResult);
    }

    public async Task<ApiResponse<PagedResult<CollegeResponseDto>>> GetPendingCollegesAsync(PaginationParams paginationParams)
    {
        var pagedResult = await _collegeRepository.GetAllAsync(paginationParams, CollegeStatus.Pending);
        return ApiResponse<PagedResult<CollegeResponseDto>>.SuccessResult(pagedResult);
    }

    public async Task<ApiResponse<CollegeResponseDto>> GetCollegeByIdAsync(int collegeId)
    {
        var college = await _collegeRepository.GetByIdAsync(collegeId);
        if (college == null)
        {
            return ApiResponse<CollegeResponseDto>.FailureResult("College not found.");
        }

        return ApiResponse<CollegeResponseDto>.SuccessResult(MapToResponseDto(college));
    }

    public async Task<ApiResponse<CollegeResponseDto>> UpdateCollegeAsync(int collegeId, UpdateCollegeRequestDto request, string currentUserId, string currentUserRole)
    {
        var college = await _collegeRepository.GetByIdAsync(collegeId);
        if (college == null)
        {
            return ApiResponse<CollegeResponseDto>.FailureResult("College not found.");
        }

        // Ownership / Authorization validation
        if (currentUserRole != AppRoles.SuperAdmin)
        {
            var officer = await _context.PlacementOfficers
                .FirstOrDefaultAsync(po => po.UserId == currentUserId && po.CollegeId == collegeId);

            if (officer == null)
            {
                return ApiResponse<CollegeResponseDto>.FailureResult("You are not authorized to update this college.");
            }
        }

        var emailExists = await _collegeRepository.OfficialEmailExistsAsync(request.OfficialEmail, collegeId);
        if (emailExists)
        {
            return ApiResponse<CollegeResponseDto>.FailureResult("Another college is already registered with this official email.");
        }

        college.CollegeName = request.CollegeName.Trim();
        college.UniversityName = request.UniversityName.Trim();
        college.OfficialEmail = request.OfficialEmail.Trim().ToLower();
        college.PhoneNumber = request.PhoneNumber.Trim();
        college.Address = request.Address.Trim();
        college.Website = request.Website?.Trim();

        await _collegeRepository.UpdateAsync(college);
        _logger.LogInformation("College ID {CollegeId} updated by user {UserId}", collegeId, currentUserId);

        return ApiResponse<CollegeResponseDto>.SuccessResult(MapToResponseDto(college), "College details updated successfully.");
    }

    public async Task<ApiResponse<bool>> ApproveCollegeAsync(int collegeId)
    {
        var college = await _collegeRepository.GetByIdAsync(collegeId);
        if (college == null)
        {
            return ApiResponse<bool>.FailureResult("College not found.");
        }

        if (college.Status == CollegeStatus.Approved)
        {
            return ApiResponse<bool>.FailureResult("College is already approved.");
        }

        college.Status = CollegeStatus.Approved;
        await _collegeRepository.UpdateAsync(college);
        _logger.LogInformation("College ID {CollegeId} approved by SuperAdmin", collegeId);

        return ApiResponse<bool>.SuccessResult(true, "College approved successfully.");
    }

    public async Task<ApiResponse<bool>> RejectCollegeAsync(int collegeId)
    {
        var college = await _collegeRepository.GetByIdAsync(collegeId);
        if (college == null)
        {
            return ApiResponse<bool>.FailureResult("College not found.");
        }

        college.Status = CollegeStatus.Rejected;
        await _collegeRepository.UpdateAsync(college);
        _logger.LogInformation("College ID {CollegeId} rejected by SuperAdmin", collegeId);

        return ApiResponse<bool>.SuccessResult(true, "College rejected successfully.");
    }

    public async Task<ApiResponse<bool>> SuspendCollegeAsync(int collegeId)
    {
        var college = await _collegeRepository.GetByIdAsync(collegeId);
        if (college == null)
        {
            return ApiResponse<bool>.FailureResult("College not found.");
        }

        college.Status = CollegeStatus.Suspended;
        await _collegeRepository.UpdateAsync(college);
        _logger.LogInformation("College ID {CollegeId} suspended by SuperAdmin", collegeId);

        return ApiResponse<bool>.SuccessResult(true, "College suspended successfully.");
    }

    private static CollegeResponseDto MapToResponseDto(College c)
    {
        return new CollegeResponseDto
        {
            CollegeId = c.CollegeId,
            CollegeName = c.CollegeName,
            CollegeCode = c.CollegeCode,
            UniversityName = c.UniversityName,
            OfficialEmail = c.OfficialEmail,
            PhoneNumber = c.PhoneNumber,
            Address = c.Address,
            Website = c.Website,
            Status = c.Status,
            CreatedAt = c.CreatedAt,
            TotalPlacementOfficers = c.PlacementOfficers?.Count ?? 0,
            TotalStudents = c.Students?.Count ?? 0
        };
    }
}
