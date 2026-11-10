using CampusPlacementSystem.Data;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Interview;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementSystem.Services.Implementations;

public class InterviewService : IInterviewService
{
    private readonly IInterviewRepository _interviewRepository;
    private readonly IJobRepository _jobRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly ICompanyHRRepository _hrRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly IPlacementOfficerRepository _officerRepository;
    private readonly AppDbContext _context;
    private readonly ILogger<InterviewService> _logger;

    public InterviewService(
        IInterviewRepository interviewRepository,
        IJobRepository jobRepository,
        IApplicationRepository applicationRepository,
        ICompanyHRRepository hrRepository,
        IStudentRepository studentRepository,
        IPlacementOfficerRepository officerRepository,
        AppDbContext context,
        ILogger<InterviewService> logger)
    {
        _interviewRepository = interviewRepository;
        _jobRepository = jobRepository;
        _applicationRepository = applicationRepository;
        _hrRepository = hrRepository;
        _studentRepository = studentRepository;
        _officerRepository = officerRepository;
        _context = context;
        _logger = logger;
    }

    // --- Interview Rounds ---

    public async Task<ApiResponse<InterviewRoundResponseDto>> CreateRoundAsync(CreateInterviewRoundRequestDto request, string hrUserId, string currentUserRole)
    {
        var job = await _jobRepository.GetByIdAsync(request.JobId);
        if (job == null)
        {
            return ApiResponse<InterviewRoundResponseDto>.FailureResult("Job opening not found.");
        }

        if (currentUserRole != AppRoles.SuperAdmin)
        {
            var hr = await _hrRepository.GetByUserIdAsync(hrUserId);
            if (hr == null || !hr.ApprovedAt.HasValue || hr.User.AccountStatus != AccountStatus.Active)
            {
                return ApiResponse<InterviewRoundResponseDto>.FailureResult("Only approved and active Company HR can create interview rounds.");
            }

            if (job.CompanyId != hr.CompanyId)
            {
                return ApiResponse<InterviewRoundResponseDto>.FailureResult("You can only create interview rounds for your company's jobs.");
            }
        }

        var roundExists = await _interviewRepository.RoundNumberExistsForJobAsync(request.JobId, request.RoundNumber);
        if (roundExists)
        {
            return ApiResponse<InterviewRoundResponseDto>.FailureResult($"Round number {request.RoundNumber} already exists for this job opening.");
        }

        var round = new InterviewRound
        {
            JobId = request.JobId,
            RoundName = request.RoundName.Trim(),
            RoundNumber = request.RoundNumber,
            Description = request.Description?.Trim()
        };

        await _interviewRepository.AddRoundAsync(round);

        _logger.LogInformation("Interview round {RoundNumber} ('{RoundName}') created for Job ID {JobId}",
            round.RoundNumber, round.RoundName, round.JobId);

        var response = new InterviewRoundResponseDto
        {
            InterviewRoundId = round.InterviewRoundId,
            JobId = round.JobId,
            JobTitle = job.JobTitle,
            RoundName = round.RoundName,
            RoundNumber = round.RoundNumber,
            Description = round.Description,
            TotalScheduledInterviews = 0
        };

        return ApiResponse<InterviewRoundResponseDto>.SuccessResult(response, "Interview round created successfully.");
    }

    public async Task<ApiResponse<List<InterviewRoundResponseDto>>> GetRoundsByJobIdAsync(int jobId, string currentUserId, string currentUserRole)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null)
        {
            return ApiResponse<List<InterviewRoundResponseDto>>.FailureResult("Job opening not found.");
        }

        var rounds = await _interviewRepository.GetRoundsByJobIdAsync(jobId);

        var result = rounds.Select(r => new InterviewRoundResponseDto
        {
            InterviewRoundId = r.InterviewRoundId,
            JobId = r.JobId,
            JobTitle = job.JobTitle,
            RoundName = r.RoundName,
            RoundNumber = r.RoundNumber,
            Description = r.Description,
            TotalScheduledInterviews = r.InterviewSchedules.Count
        }).ToList();

        return ApiResponse<List<InterviewRoundResponseDto>>.SuccessResult(result);
    }

    // --- Interview Schedules ---

    public async Task<ApiResponse<InterviewScheduleResponseDto>> ScheduleInterviewAsync(ScheduleInterviewRequestDto request, string hrUserId, string currentUserRole)
    {
        var application = await _applicationRepository.GetByIdWithDetailsAsync(request.ApplicationId);
        if (application == null)
        {
            return ApiResponse<InterviewScheduleResponseDto>.FailureResult("Job application not found.");
        }

        if (currentUserRole != AppRoles.SuperAdmin)
        {
            var hr = await _hrRepository.GetByUserIdAsync(hrUserId);
            if (hr == null || !hr.ApprovedAt.HasValue || hr.User.AccountStatus != AccountStatus.Active)
            {
                return ApiResponse<InterviewScheduleResponseDto>.FailureResult("Only approved and active Company HR can schedule interviews.");
            }

            if (application.JobOpening.CompanyId != hr.CompanyId)
            {
                return ApiResponse<InterviewScheduleResponseDto>.FailureResult("You can only schedule interviews for your company's applicants.");
            }
        }

        if (application.Status == ApplicationStatus.Withdrawn ||
            application.Status == ApplicationStatus.Rejected ||
            application.Status == ApplicationStatus.NotSelected)
        {
            return ApiResponse<InterviewScheduleResponseDto>.FailureResult($"Cannot schedule an interview for an application in status '{application.Status}'.");
        }

        var round = await _interviewRepository.GetRoundByIdAsync(request.InterviewRoundId);
        if (round == null)
        {
            return ApiResponse<InterviewScheduleResponseDto>.FailureResult("Interview round not found.");
        }

        if (round.JobId != application.JobId)
        {
            return ApiResponse<InterviewScheduleResponseDto>.FailureResult("The interview round does not belong to the job opening applied for.");
        }

        var scheduledUtc = request.ScheduledAt.Kind == DateTimeKind.Utc 
            ? request.ScheduledAt 
            : request.ScheduledAt.ToUniversalTime();

        if (scheduledUtc <= DateTime.UtcNow)
        {
            return ApiResponse<InterviewScheduleResponseDto>.FailureResult("Interview must be scheduled for a future date and time.");
        }

        var hasActive = await _interviewRepository.HasActiveScheduleForRoundAsync(request.ApplicationId, request.InterviewRoundId);
        if (hasActive)
        {
            return ApiResponse<InterviewScheduleResponseDto>.FailureResult("An active interview schedule already exists for this candidate in this round.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var schedule = new InterviewSchedule
            {
                ApplicationId = request.ApplicationId,
                InterviewRoundId = request.InterviewRoundId,
                ScheduledAt = scheduledUtc,
                MeetingLink = request.MeetingLink?.Trim(),
                Venue = request.Venue?.Trim(),
                Status = InterviewStatus.Scheduled,
                CreatedAt = DateTime.UtcNow
            };

            await _interviewRepository.AddScheduleAsync(schedule);

            // Automatically transition application status to InterviewScheduled if not already
            if (application.Status != ApplicationStatus.InterviewScheduled)
            {
                var previousStatus = application.Status;
                application.Status = ApplicationStatus.InterviewScheduled;
                application.LastUpdatedAt = DateTime.UtcNow;

                await _applicationRepository.UpdateAsync(application);

                var history = new ApplicationStatusHistory
                {
                    ApplicationId = application.ApplicationId,
                    PreviousStatus = previousStatus,
                    NewStatus = ApplicationStatus.InterviewScheduled,
                    UpdatedByUserId = hrUserId,
                    Remarks = $"Interview scheduled for round '{round.RoundName}' (#{round.RoundNumber}).",
                    UpdatedAt = DateTime.UtcNow
                };

                await _applicationRepository.AddStatusHistoryAsync(history);
            }

            await transaction.CommitAsync();

            _logger.LogInformation("Interview scheduled: ID {ScheduleId} for Application {AppId}, Round {RoundId}",
                schedule.InterviewScheduleId, schedule.ApplicationId, schedule.InterviewRoundId);

            var response = MapToScheduleResponseDto(schedule, application, round);
            return ApiResponse<InterviewScheduleResponseDto>.SuccessResult(response, "Interview scheduled successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to schedule interview.");
            throw;
        }
    }

    public async Task<ApiResponse<InterviewScheduleResponseDto>> UpdateScheduleAsync(int scheduleId, UpdateInterviewScheduleRequestDto request, string hrUserId, string currentUserRole)
    {
        var schedule = await _interviewRepository.GetScheduleByIdWithDetailsAsync(scheduleId);
        if (schedule == null)
        {
            return ApiResponse<InterviewScheduleResponseDto>.FailureResult("Interview schedule not found.");
        }

        if (currentUserRole != AppRoles.SuperAdmin)
        {
            var hr = await _hrRepository.GetByUserIdAsync(hrUserId);
            if (hr == null || hr.CompanyId != schedule.Application.JobOpening.CompanyId)
            {
                return ApiResponse<InterviewScheduleResponseDto>.FailureResult("You can only update interview schedules for your company's jobs.");
            }
        }

        if (schedule.Status == InterviewStatus.Cancelled)
        {
            return ApiResponse<InterviewScheduleResponseDto>.FailureResult("Cannot modify a cancelled interview schedule.");
        }

        var scheduledUtc = request.ScheduledAt.Kind == DateTimeKind.Utc 
            ? request.ScheduledAt 
            : request.ScheduledAt.ToUniversalTime();

        schedule.ScheduledAt = scheduledUtc;
        schedule.MeetingLink = request.MeetingLink?.Trim();
        schedule.Venue = request.Venue?.Trim();
        schedule.Status = request.Status;

        await _interviewRepository.UpdateScheduleAsync(schedule);

        _logger.LogInformation("Interview schedule ID {ScheduleId} updated. Status: {Status}", schedule.InterviewScheduleId, schedule.Status);

        var response = MapToScheduleResponseDto(schedule, schedule.Application, schedule.InterviewRound);
        return ApiResponse<InterviewScheduleResponseDto>.SuccessResult(response, "Interview schedule updated successfully.");
    }

    public async Task<ApiResponse<bool>> SubmitFeedbackAsync(int scheduleId, SubmitInterviewFeedbackRequestDto request, string hrUserId, string currentUserRole)
    {
        var schedule = await _interviewRepository.GetScheduleByIdWithDetailsAsync(scheduleId);
        if (schedule == null)
        {
            return ApiResponse<bool>.FailureResult("Interview schedule not found.");
        }

        if (currentUserRole != AppRoles.SuperAdmin)
        {
            var hr = await _hrRepository.GetByUserIdAsync(hrUserId);
            if (hr == null || hr.CompanyId != schedule.Application.JobOpening.CompanyId)
            {
                return ApiResponse<bool>.FailureResult("You can only submit feedback for your company's interviews.");
            }
        }

        if (schedule.Status == InterviewStatus.Cancelled)
        {
            return ApiResponse<bool>.FailureResult("Cannot submit feedback for a cancelled interview.");
        }

        schedule.Feedback = request.Feedback.Trim();
        schedule.Status = request.Status;

        await _interviewRepository.UpdateScheduleAsync(schedule);

        _logger.LogInformation("Interview schedule ID {ScheduleId} feedback recorded. Status: {Status}", schedule.InterviewScheduleId, schedule.Status);
        return ApiResponse<bool>.SuccessResult(true, "Interview feedback submitted successfully.");
    }

    public async Task<ApiResponse<InterviewScheduleResponseDto>> GetScheduleByIdAsync(int scheduleId, string currentUserId, string currentUserRole)
    {
        var schedule = await _interviewRepository.GetScheduleByIdWithDetailsAsync(scheduleId);
        if (schedule == null)
        {
            return ApiResponse<InterviewScheduleResponseDto>.FailureResult("Interview schedule not found.");
        }

        // Authorization checks
        if (currentUserRole == AppRoles.Student)
        {
            var student = await _studentRepository.GetByUserIdAsync(currentUserId);
            if (student == null || student.StudentId != schedule.Application.StudentId)
            {
                return ApiResponse<InterviewScheduleResponseDto>.FailureResult("You are not authorized to view another candidate's interview schedule.");
            }
        }
        else if (currentUserRole == AppRoles.PlacementOfficer)
        {
            var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
            if (officer == null || officer.CollegeId != schedule.Application.Student.CollegeId)
            {
                return ApiResponse<InterviewScheduleResponseDto>.FailureResult("You can only view interview schedules for students from your college.");
            }
        }
        else if (currentUserRole == AppRoles.CompanyHR)
        {
            var hr = await _hrRepository.GetByUserIdAsync(currentUserId);
            if (hr == null || hr.CompanyId != schedule.Application.JobOpening.CompanyId)
            {
                return ApiResponse<InterviewScheduleResponseDto>.FailureResult("You are not authorized to view interview schedules for other companies.");
            }
        }

        var response = MapToScheduleResponseDto(schedule, schedule.Application, schedule.InterviewRound);
        return ApiResponse<InterviewScheduleResponseDto>.SuccessResult(response);
    }

    public async Task<ApiResponse<PagedResult<InterviewScheduleResponseDto>>> GetSchedulesAsync(InterviewFilterParams filterParams, string currentUserId, string currentUserRole)
    {
        int? companyId = null;
        int? studentId = null;
        int? collegeId = null;

        if (currentUserRole == AppRoles.CompanyHR)
        {
            var hr = await _hrRepository.GetByUserIdAsync(currentUserId);
            if (hr == null)
            {
                return ApiResponse<PagedResult<InterviewScheduleResponseDto>>.FailureResult("Company HR profile not found.");
            }
            companyId = hr.CompanyId;
        }
        else if (currentUserRole == AppRoles.PlacementOfficer)
        {
            var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
            if (officer == null)
            {
                return ApiResponse<PagedResult<InterviewScheduleResponseDto>>.FailureResult("Placement officer profile not found.");
            }
            collegeId = officer.CollegeId;
        }
        else if (currentUserRole == AppRoles.Student)
        {
            var student = await _studentRepository.GetByUserIdAsync(currentUserId);
            if (student == null)
            {
                return ApiResponse<PagedResult<InterviewScheduleResponseDto>>.FailureResult("Student profile not found.");
            }
            studentId = student.StudentId;
        }

        var result = await _interviewRepository.GetSchedulesAsync(filterParams, companyId, studentId, collegeId);
        return ApiResponse<PagedResult<InterviewScheduleResponseDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<bool>> CancelScheduleAsync(int scheduleId, string hrUserId, string currentUserRole)
    {
        var schedule = await _interviewRepository.GetScheduleByIdWithDetailsAsync(scheduleId);
        if (schedule == null)
        {
            return ApiResponse<bool>.FailureResult("Interview schedule not found.");
        }

        if (currentUserRole != AppRoles.SuperAdmin)
        {
            var hr = await _hrRepository.GetByUserIdAsync(hrUserId);
            if (hr == null || hr.CompanyId != schedule.Application.JobOpening.CompanyId)
            {
                return ApiResponse<bool>.FailureResult("You can only cancel interview schedules for your company's jobs.");
            }
        }

        if (schedule.Status == InterviewStatus.Cancelled)
        {
            return ApiResponse<bool>.FailureResult("This interview schedule has already been cancelled.");
        }

        schedule.Status = InterviewStatus.Cancelled;
        await _interviewRepository.UpdateScheduleAsync(schedule);

        _logger.LogInformation("Interview schedule ID {ScheduleId} cancelled by {UserId}", scheduleId, hrUserId);
        return ApiResponse<bool>.SuccessResult(true, "Interview schedule cancelled successfully.");
    }

    private static InterviewScheduleResponseDto MapToScheduleResponseDto(InterviewSchedule s, JobApplication a, InterviewRound r)
    {
        return new InterviewScheduleResponseDto
        {
            InterviewScheduleId = s.InterviewScheduleId,
            ApplicationId = s.ApplicationId,
            StudentId = a.StudentId,
            StudentName = a.Student?.User?.FullName ?? string.Empty,
            StudentEmail = a.Student?.User?.Email ?? string.Empty,
            RegisterNumber = a.Student?.RegisterNumber ?? string.Empty,
            CollegeId = a.Student?.CollegeId ?? 0,
            CollegeName = a.Student?.College?.CollegeName ?? string.Empty,
            JobId = a.JobId,
            JobTitle = a.JobOpening?.JobTitle ?? string.Empty,
            CompanyId = a.JobOpening?.CompanyId ?? 0,
            CompanyName = a.JobOpening?.Company?.CompanyName ?? string.Empty,
            InterviewRoundId = s.InterviewRoundId,
            RoundName = r.RoundName,
            RoundNumber = r.RoundNumber,
            ScheduledAt = s.ScheduledAt,
            MeetingLink = s.MeetingLink,
            Venue = s.Venue,
            Status = s.Status,
            Feedback = s.Feedback,
            CreatedAt = s.CreatedAt
        };
    }
}
