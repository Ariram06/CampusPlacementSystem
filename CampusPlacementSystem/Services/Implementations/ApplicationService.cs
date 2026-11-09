using CampusPlacementSystem.Data;
using CampusPlacementSystem.DTOs.Application;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementSystem.Services.Implementations;

public class ApplicationService : IApplicationService
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICompanyHRRepository _hrRepository;
    private readonly AppDbContext _context;
    private readonly ILogger<ApplicationService> _logger;

    public ApplicationService(
        IApplicationRepository applicationRepository,
        IJobRepository jobRepository,
        IStudentRepository studentRepository,
        ICompanyHRRepository hrRepository,
        AppDbContext context,
        ILogger<ApplicationService> logger)
    {
        _applicationRepository = applicationRepository;
        _jobRepository = jobRepository;
        _studentRepository = studentRepository;
        _hrRepository = hrRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<ApplicationResponseDto>> ApplyJobAsync(ApplyJobRequestDto request, string studentUserId)
    {
        var student = await _studentRepository.GetByUserIdAsync(studentUserId);
        if (student == null || student.Status != StudentStatus.Approved || student.User.AccountStatus != AccountStatus.Active)
        {
            return ApiResponse<ApplicationResponseDto>.FailureResult("Only approved and active students can apply for job openings.");
        }

        var job = await _jobRepository.GetByIdWithDetailsAsync(request.JobId);
        if (job == null)
        {
            return ApiResponse<ApplicationResponseDto>.FailureResult("Job opening not found.");
        }

        if (job.Status != JobStatus.Published)
        {
            return ApiResponse<ApplicationResponseDto>.FailureResult("Applications are only accepted for published job openings.");
        }

        if (job.ApplicationDeadline <= DateTime.UtcNow)
        {
            return ApiResponse<ApplicationResponseDto>.FailureResult("The application deadline for this job opening has passed.");
        }

        var alreadyApplied = await _applicationRepository.HasStudentAppliedAsync(student.StudentId, job.JobId);
        if (alreadyApplied)
        {
            return ApiResponse<ApplicationResponseDto>.FailureResult("You have already submitted an application for this job opening.");
        }

        // Reusable Eligibility Evaluation check
        var eligibility = JobService.EvaluateEligibility(job, student);
        if (!eligibility.IsEligible)
        {
            var reasons = string.Join("; ", eligibility.FailedCriteria);
            return ApiResponse<ApplicationResponseDto>.FailureResult($"You do not meet the eligibility requirements for this job: {reasons}");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var application = new JobApplication
            {
                JobId = job.JobId,
                StudentId = student.StudentId,
                AppliedAt = DateTime.UtcNow,
                Status = ApplicationStatus.Applied,
                ResumeUrl = !string.IsNullOrWhiteSpace(request.ResumeUrl) ? request.ResumeUrl.Trim() : student.ResumeUrl
            };

            await _applicationRepository.AddAsync(application);

            var initialHistory = new ApplicationStatusHistory
            {
                ApplicationId = application.ApplicationId,
                PreviousStatus = ApplicationStatus.Applied,
                NewStatus = ApplicationStatus.Applied,
                UpdatedByUserId = student.UserId,
                Remarks = "Application submitted by student.",
                UpdatedAt = DateTime.UtcNow
            };

            await _applicationRepository.AddStatusHistoryAsync(initialHistory);
            await transaction.CommitAsync();

            _logger.LogInformation("Student ID {StudentId} applied for Job ID {JobId}. ApplicationId: {AppId}", student.StudentId, job.JobId, application.ApplicationId);

            var response = MapToResponseDto(application, job, student);
            return ApiResponse<ApplicationResponseDto>.SuccessResult(response, "Application submitted successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to submit job application.");
            throw;
        }
    }

    public async Task<ApiResponse<PagedResult<ApplicationResponseDto>>> GetMyApplicationsAsync(string studentUserId, ApplicationFilterParams filterParams)
    {
        var student = await _studentRepository.GetByUserIdAsync(studentUserId);
        if (student == null)
        {
            return ApiResponse<PagedResult<ApplicationResponseDto>>.FailureResult("Student profile not found.");
        }

        var result = await _applicationRepository.GetApplicationsAsync(filterParams, studentId: student.StudentId);
        return ApiResponse<PagedResult<ApplicationResponseDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<PagedResult<ApplicationResponseDto>>> GetJobApplicantsAsync(int jobId, ApplicationFilterParams filterParams, string hrUserId, string currentUserRole)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null)
        {
            return ApiResponse<PagedResult<ApplicationResponseDto>>.FailureResult("Job opening not found.");
        }

        if (currentUserRole != AppRoles.SuperAdmin)
        {
            var hr = await _hrRepository.GetByUserIdAsync(hrUserId);
            if (hr == null || hr.CompanyId != job.CompanyId)
            {
                return ApiResponse<PagedResult<ApplicationResponseDto>>.FailureResult("You can only view applicants for your company's jobs.");
            }
        }

        filterParams.JobId = jobId;
        var result = await _applicationRepository.GetApplicationsAsync(filterParams, companyId: job.CompanyId);
        return ApiResponse<PagedResult<ApplicationResponseDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<ApplicationResponseDto>> GetApplicationByIdAsync(int id, string currentUserId, string currentUserRole)
    {
        var app = await _applicationRepository.GetByIdWithDetailsAsync(id);
        if (app == null)
        {
            return ApiResponse<ApplicationResponseDto>.FailureResult("Application not found.");
        }

        // Ownership / Authorization verification
        if (currentUserRole == AppRoles.Student && app.Student.UserId != currentUserId)
        {
            return ApiResponse<ApplicationResponseDto>.FailureResult("You are not authorized to view another student's application.");
        }

        if (currentUserRole == AppRoles.CompanyHR)
        {
            var hr = await _hrRepository.GetByUserIdAsync(currentUserId);
            if (hr == null || hr.CompanyId != app.JobOpening.CompanyId)
            {
                return ApiResponse<ApplicationResponseDto>.FailureResult("You are not authorized to view applications for other companies.");
            }
        }

        if (currentUserRole == AppRoles.PlacementOfficer)
        {
            var officer = await _context.PlacementOfficers.FirstOrDefaultAsync(po => po.UserId == currentUserId);
            if (officer == null || officer.CollegeId != app.Student.CollegeId)
            {
                return ApiResponse<ApplicationResponseDto>.FailureResult("You can only view applications for students from your college.");
            }
        }

        return ApiResponse<ApplicationResponseDto>.SuccessResult(MapToResponseDto(app, app.JobOpening, app.Student));
    }

    public async Task<ApiResponse<bool>> UpdateApplicationStatusAsync(int id, UpdateApplicationStatusRequestDto request, string hrUserId, string currentUserRole)
    {
        var application = await _applicationRepository.GetByIdWithDetailsAsync(id);
        if (application == null)
        {
            return ApiResponse<bool>.FailureResult("Application not found.");
        }

        if (currentUserRole != AppRoles.SuperAdmin)
        {
            var hr = await _hrRepository.GetByUserIdAsync(hrUserId);
            if (hr == null || hr.CompanyId != application.JobOpening.CompanyId)
            {
                return ApiResponse<bool>.FailureResult("You can only update application decisions for your company's jobs.");
            }
        }

        if (application.Status == ApplicationStatus.Withdrawn)
        {
            return ApiResponse<bool>.FailureResult("Cannot update the status of a withdrawn application.");
        }

        var previousStatus = application.Status;
        if (previousStatus == request.Status)
        {
            return ApiResponse<bool>.FailureResult($"Application is already in status '{request.Status}'.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            application.Status = request.Status;
            application.LastUpdatedAt = DateTime.UtcNow;

            await _applicationRepository.UpdateAsync(application);

            var history = new ApplicationStatusHistory
            {
                ApplicationId = application.ApplicationId,
                PreviousStatus = previousStatus,
                NewStatus = request.Status,
                UpdatedByUserId = hrUserId,
                Remarks = request.Remarks?.Trim(),
                UpdatedAt = DateTime.UtcNow
            };

            await _applicationRepository.AddStatusHistoryAsync(history);
            await transaction.CommitAsync();

            _logger.LogInformation("Application ID {AppId} status updated: {OldStatus} -> {NewStatus}", id, previousStatus, request.Status);
            return ApiResponse<bool>.SuccessResult(true, $"Application status updated to '{request.Status}' successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to update application status.");
            throw;
        }
    }

    public async Task<ApiResponse<bool>> WithdrawApplicationAsync(int id, string studentUserId)
    {
        var application = await _applicationRepository.GetByIdAsync(id);
        if (application == null)
        {
            return ApiResponse<bool>.FailureResult("Application not found.");
        }

        if (application.Student.UserId != studentUserId)
        {
            return ApiResponse<bool>.FailureResult("You can only withdraw your own applications.");
        }

        if (application.Status == ApplicationStatus.Withdrawn)
        {
            return ApiResponse<bool>.FailureResult("This application has already been withdrawn.");
        }

        if (application.Status == ApplicationStatus.Selected)
        {
            return ApiResponse<bool>.FailureResult("Cannot withdraw an application that has already been selected.");
        }

        var previousStatus = application.Status;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            application.Status = ApplicationStatus.Withdrawn;
            application.LastUpdatedAt = DateTime.UtcNow;

            await _applicationRepository.UpdateAsync(application);

            var history = new ApplicationStatusHistory
            {
                ApplicationId = application.ApplicationId,
                PreviousStatus = previousStatus,
                NewStatus = ApplicationStatus.Withdrawn,
                UpdatedByUserId = studentUserId,
                Remarks = "Application withdrawn by candidate.",
                UpdatedAt = DateTime.UtcNow
            };

            await _applicationRepository.AddStatusHistoryAsync(history);
            await transaction.CommitAsync();

            _logger.LogInformation("Application ID {AppId} withdrawn by student {UserId}", id, studentUserId);
            return ApiResponse<bool>.SuccessResult(true, "Application withdrawn successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to withdraw application.");
            throw;
        }
    }

    public async Task<ApiResponse<List<ApplicationStatusHistoryResponseDto>>> GetApplicationStatusHistoryAsync(int id, string currentUserId, string currentUserRole)
    {
        var application = await _applicationRepository.GetByIdAsync(id);
        if (application == null)
        {
            return ApiResponse<List<ApplicationStatusHistoryResponseDto>>.FailureResult("Application not found.");
        }

        // Authorization checks
        if (currentUserRole == AppRoles.Student && application.Student.UserId != currentUserId)
        {
            return ApiResponse<List<ApplicationStatusHistoryResponseDto>>.FailureResult("You are not authorized to view status history for this application.");
        }

        if (currentUserRole == AppRoles.CompanyHR)
        {
            var hr = await _hrRepository.GetByUserIdAsync(currentUserId);
            if (hr == null || hr.CompanyId != application.JobOpening.CompanyId)
            {
                return ApiResponse<List<ApplicationStatusHistoryResponseDto>>.FailureResult("You are not authorized to view status history for this company's applicants.");
            }
        }

        var history = await _applicationRepository.GetStatusHistoryAsync(id);

        var result = history.Select(h => new ApplicationStatusHistoryResponseDto
        {
            HistoryId = h.HistoryId,
            ApplicationId = h.ApplicationId,
            PreviousStatus = h.PreviousStatus,
            NewStatus = h.NewStatus,
            UpdatedByUserId = h.UpdatedByUserId,
            UpdatedByUserName = h.UpdatedByUser?.FullName ?? "System",
            Remarks = h.Remarks,
            UpdatedAt = h.UpdatedAt
        }).ToList();

        return ApiResponse<List<ApplicationStatusHistoryResponseDto>>.SuccessResult(result);
    }

    private static ApplicationResponseDto MapToResponseDto(JobApplication a, JobOpening j, Student s)
    {
        return new ApplicationResponseDto
        {
            ApplicationId = a.ApplicationId,
            JobId = a.JobId,
            JobTitle = j.JobTitle,
            CompanyId = j.CompanyId,
            CompanyName = j.Company?.CompanyName ?? string.Empty,
            StudentId = s.StudentId,
            StudentName = s.User?.FullName ?? string.Empty,
            StudentEmail = s.User?.Email ?? string.Empty,
            RegisterNumber = s.RegisterNumber,
            Department = s.Department,
            Degree = s.Degree,
            GraduationYear = s.GraduationYear,
            CGPA = s.CGPA,
            ActiveBacklogs = s.ActiveBacklogs,
            CollegeId = s.CollegeId,
            CollegeName = s.College?.CollegeName ?? string.Empty,
            AppliedAt = a.AppliedAt,
            Status = a.Status,
            ResumeUrl = a.ResumeUrl,
            LastUpdatedAt = a.LastUpdatedAt,
            TotalInterviewSchedules = a.InterviewSchedules?.Count ?? 0
        };
    }
}
