using CampusPlacementSystem.Data;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.RecruitmentDrive;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementSystem.Services.Implementations;

public class RecruitmentDriveService : IRecruitmentDriveService
{
    private readonly IRecruitmentDriveRepository _driveRepository;
    private readonly IJobRepository _jobRepository;
    private readonly ICollegeRepository _collegeRepository;
    private readonly ICompanyHRRepository _hrRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IPlacementOfficerRepository _officerRepository;
    private readonly ILogger<RecruitmentDriveService> _logger;

    public RecruitmentDriveService(
        IRecruitmentDriveRepository driveRepository,
        IJobRepository jobRepository,
        ICollegeRepository collegeRepository,
        ICompanyHRRepository hrRepository,
        IStudentRepository studentRepository,
        IPlacementOfficerRepository officerRepository,
        ILogger<RecruitmentDriveService> logger)
    {
        _driveRepository = driveRepository;
        _jobRepository = jobRepository;
        _collegeRepository = collegeRepository;
        _hrRepository = hrRepository;
        _studentRepository = studentRepository;
        _officerRepository = officerRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<RecruitmentDriveResponseDto>> CreateDriveAsync(CreateRecruitmentDriveRequestDto request, string hrUserId, string currentUserRole)
    {
        var job = await _jobRepository.GetByIdWithDetailsAsync(request.JobId);
        if (job == null)
        {
            return ApiResponse<RecruitmentDriveResponseDto>.FailureResult("Job opening not found.");
        }

        if (currentUserRole != AppRoles.SuperAdmin)
        {
            var hr = await _hrRepository.GetByUserIdAsync(hrUserId);
            if (hr == null || !hr.ApprovedAt.HasValue || hr.User.AccountStatus != AccountStatus.Active)
            {
                return ApiResponse<RecruitmentDriveResponseDto>.FailureResult("Only approved and active Company HR can create recruitment drives.");
            }

            if (hr.Company.Status != CompanyStatus.Approved)
            {
                return ApiResponse<RecruitmentDriveResponseDto>.FailureResult("Cannot schedule recruitment drives for a company that is not approved or is suspended.");
            }

            if (job.CompanyId != hr.CompanyId)
            {
                return ApiResponse<RecruitmentDriveResponseDto>.FailureResult("You can only create recruitment drives for your company's jobs.");
            }
        }

        if (job.Status != JobStatus.Published)
        {
            return ApiResponse<RecruitmentDriveResponseDto>.FailureResult("Recruitment drives can only be scheduled for published job openings.");
        }

        var college = await _collegeRepository.GetByIdAsync(request.CollegeId);
        if (college == null || college.Status != CollegeStatus.Approved)
        {
            return ApiResponse<RecruitmentDriveResponseDto>.FailureResult("Target college not found or is not approved.");
        }

        if (!job.IsOpenToAllColleges && !job.TargetColleges.Any(tc => tc.CollegeId == request.CollegeId))
        {
            return ApiResponse<RecruitmentDriveResponseDto>.FailureResult($"College '{college.CollegeName}' is not targeted by this job opening.");
        }

        if (request.DriveDate <= DateTime.UtcNow)
        {
            return ApiResponse<RecruitmentDriveResponseDto>.FailureResult("Recruitment drive date must be scheduled for a future date and time.");
        }

        var drive = new RecruitmentDrive
        {
            JobId = request.JobId,
            CollegeId = request.CollegeId,
            DriveDate = request.DriveDate,
            Venue = request.Venue.Trim(),
            Description = request.Description?.Trim(),
            Status = DriveStatus.Scheduled,
            CreatedAt = DateTime.UtcNow
        };

        await _driveRepository.AddAsync(drive);

        _logger.LogInformation("Recruitment drive scheduled: Drive ID {DriveId} for Job {JobId} at College {CollegeId}",
            drive.DriveId, drive.JobId, drive.CollegeId);

        var response = MapToResponseDto(drive, job, college);
        return ApiResponse<RecruitmentDriveResponseDto>.SuccessResult(response, "Recruitment drive scheduled successfully.");
    }

    public async Task<ApiResponse<RecruitmentDriveResponseDto>> UpdateDriveAsync(int id, UpdateRecruitmentDriveRequestDto request, string hrUserId, string currentUserRole)
    {
        var drive = await _driveRepository.GetByIdWithDetailsAsync(id);
        if (drive == null)
        {
            return ApiResponse<RecruitmentDriveResponseDto>.FailureResult("Recruitment drive not found.");
        }

        if (currentUserRole != AppRoles.SuperAdmin)
        {
            var hr = await _hrRepository.GetByUserIdAsync(hrUserId);
            if (hr == null || hr.CompanyId != drive.JobOpening.CompanyId)
            {
                return ApiResponse<RecruitmentDriveResponseDto>.FailureResult("You can only update recruitment drives for your company's jobs.");
            }
        }

        if (drive.Status == DriveStatus.Cancelled)
        {
            return ApiResponse<RecruitmentDriveResponseDto>.FailureResult("Cannot modify a cancelled recruitment drive.");
        }

        drive.DriveDate = request.DriveDate;
        drive.Venue = request.Venue.Trim();
        drive.Description = request.Description?.Trim();
        drive.Status = request.Status;

        await _driveRepository.UpdateAsync(drive);

        _logger.LogInformation("Recruitment drive ID {DriveId} updated. Status: {Status}", drive.DriveId, drive.Status);

        var response = MapToResponseDto(drive, drive.JobOpening, drive.College);
        return ApiResponse<RecruitmentDriveResponseDto>.SuccessResult(response, "Recruitment drive updated successfully.");
    }

    public async Task<ApiResponse<RecruitmentDriveResponseDto>> GetDriveByIdAsync(int id, string currentUserId, string currentUserRole)
    {
        var drive = await _driveRepository.GetByIdWithDetailsAsync(id);
        if (drive == null)
        {
            return ApiResponse<RecruitmentDriveResponseDto>.FailureResult("Recruitment drive not found.");
        }

        // Authorization checks
        if (currentUserRole == AppRoles.CompanyHR)
        {
            var hr = await _hrRepository.GetByUserIdAsync(currentUserId);
            if (hr == null || hr.CompanyId != drive.JobOpening.CompanyId)
            {
                return ApiResponse<RecruitmentDriveResponseDto>.FailureResult("You are not authorized to view recruitment drives for other companies.");
            }
        }
        else if (currentUserRole == AppRoles.PlacementOfficer)
        {
            var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
            if (officer == null || officer.CollegeId != drive.CollegeId)
            {
                return ApiResponse<RecruitmentDriveResponseDto>.FailureResult("You can only view recruitment drives scheduled for your college.");
            }
        }
        else if (currentUserRole == AppRoles.Student)
        {
            var student = await _studentRepository.GetByUserIdAsync(currentUserId);
            if (student == null || student.CollegeId != drive.CollegeId)
            {
                return ApiResponse<RecruitmentDriveResponseDto>.FailureResult("You can only view recruitment drives scheduled for your college.");
            }
        }

        return ApiResponse<RecruitmentDriveResponseDto>.SuccessResult(MapToResponseDto(drive, drive.JobOpening, drive.College));
    }

    public async Task<ApiResponse<PagedResult<RecruitmentDriveResponseDto>>> GetDrivesAsync(RecruitmentDriveFilterParams filterParams, string currentUserId, string currentUserRole)
    {
        int? companyId = null;
        int? collegeId = null;

        if (currentUserRole == AppRoles.CompanyHR)
        {
            var hr = await _hrRepository.GetByUserIdAsync(currentUserId);
            if (hr == null)
            {
                return ApiResponse<PagedResult<RecruitmentDriveResponseDto>>.FailureResult("Company HR profile not found.");
            }
            companyId = hr.CompanyId;
        }
        else if (currentUserRole == AppRoles.PlacementOfficer)
        {
            var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
            if (officer == null)
            {
                return ApiResponse<PagedResult<RecruitmentDriveResponseDto>>.FailureResult("Placement officer profile not found.");
            }
            collegeId = officer.CollegeId;
        }
        else if (currentUserRole == AppRoles.Student)
        {
            var student = await _studentRepository.GetByUserIdAsync(currentUserId);
            if (student == null)
            {
                return ApiResponse<PagedResult<RecruitmentDriveResponseDto>>.FailureResult("Student profile not found.");
            }
            collegeId = student.CollegeId;
        }

        var result = await _driveRepository.GetDrivesAsync(filterParams, companyId, collegeId);
        return ApiResponse<PagedResult<RecruitmentDriveResponseDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<bool>> CancelDriveAsync(int id, string hrUserId, string currentUserRole)
    {
        var drive = await _driveRepository.GetByIdWithDetailsAsync(id);
        if (drive == null)
        {
            return ApiResponse<bool>.FailureResult("Recruitment drive not found.");
        }

        if (currentUserRole != AppRoles.SuperAdmin)
        {
            var hr = await _hrRepository.GetByUserIdAsync(hrUserId);
            if (hr == null || hr.CompanyId != drive.JobOpening.CompanyId)
            {
                return ApiResponse<bool>.FailureResult("You can only cancel recruitment drives for your company's jobs.");
            }
        }

        if (drive.Status == DriveStatus.Cancelled)
        {
            return ApiResponse<bool>.FailureResult("This recruitment drive has already been cancelled.");
        }

        drive.Status = DriveStatus.Cancelled;
        await _driveRepository.UpdateAsync(drive);

        _logger.LogInformation("Recruitment drive ID {DriveId} cancelled by {UserId}", id, hrUserId);
        return ApiResponse<bool>.SuccessResult(true, "Recruitment drive cancelled successfully.");
    }

    private static RecruitmentDriveResponseDto MapToResponseDto(RecruitmentDrive rd, JobOpening job, College college)
    {
        return new RecruitmentDriveResponseDto
        {
            DriveId = rd.DriveId,
            JobId = rd.JobId,
            JobTitle = job.JobTitle,
            CompanyId = job.CompanyId,
            CompanyName = job.Company?.CompanyName ?? string.Empty,
            CollegeId = rd.CollegeId,
            CollegeName = college.CollegeName,
            CollegeCode = college.CollegeCode,
            DriveDate = rd.DriveDate,
            Venue = rd.Venue,
            Description = rd.Description,
            Status = rd.Status,
            CreatedAt = rd.CreatedAt
        };
    }
}
