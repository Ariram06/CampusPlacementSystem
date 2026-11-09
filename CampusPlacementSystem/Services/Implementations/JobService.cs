using CampusPlacementSystem.Data;
using CampusPlacementSystem.DTOs.College;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Job;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementSystem.Services.Implementations;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly ICompanyHRRepository _hrRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ICollegeRepository _collegeRepository;
    private readonly AppDbContext _context;
    private readonly ILogger<JobService> _logger;

    public JobService(
        IJobRepository jobRepository,
        ICompanyHRRepository hrRepository,
        IStudentRepository studentRepository,
        ICollegeRepository collegeRepository,
        AppDbContext context,
        ILogger<JobService> logger)
    {
        _jobRepository = jobRepository;
        _hrRepository = hrRepository;
        _studentRepository = studentRepository;
        _collegeRepository = collegeRepository;
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponse<JobResponseDto>> CreateJobAsync(CreateJobRequestDto request, string hrUserId)
    {
        var hr = await _hrRepository.GetByUserIdAsync(hrUserId);
        if (hr == null || hr.User.AccountStatus != AccountStatus.Active)
        {
            return ApiResponse<JobResponseDto>.FailureResult("Only approved company HR representatives can post job openings.");
        }

        if (hr.Company.Status != CompanyStatus.Approved)
        {
            return ApiResponse<JobResponseDto>.FailureResult("Cannot create jobs for a company that is not approved or is suspended.");
        }

        if (request.ApplicationDeadline <= DateTime.UtcNow)
        {
            return ApiResponse<JobResponseDto>.FailureResult("Application deadline must be a future date and time.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var job = new JobOpening
            {
                CompanyId = hr.CompanyId,
                CreatedByHRId = hr.CompanyHRId,
                JobTitle = request.JobTitle.Trim(),
                JobDescription = request.JobDescription.Trim(),
                JobType = request.JobType.Trim(),
                Location = request.Location.Trim(),
                SalaryPackage = request.SalaryPackage,
                MinimumCGPA = request.MinimumCGPA,
                MaximumBacklogs = request.MaximumBacklogs,
                ApplicationDeadline = request.ApplicationDeadline,
                IsOpenToAllColleges = request.IsOpenToAllColleges,
                Status = JobStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            await _jobRepository.AddAsync(job);

            if (request.EligibleDepartments.Any())
            {
                foreach (var dept in request.EligibleDepartments.Distinct())
                {
                    job.EligibleDepartments.Add(new JobEligibleDepartment
                    {
                        JobId = job.JobId,
                        Department = dept.Trim()
                    });
                }
            }

            if (request.EligibleGraduationYears.Any())
            {
                foreach (var year in request.EligibleGraduationYears.Distinct())
                {
                    job.EligibleGraduationYears.Add(new JobEligibleGraduationYear
                    {
                        JobId = job.JobId,
                        GraduationYear = year
                    });
                }
            }

            if (!request.IsOpenToAllColleges && request.TargetCollegeIds.Any())
            {
                foreach (var collegeId in request.TargetCollegeIds.Distinct())
                {
                    var college = await _collegeRepository.GetByIdAsync(collegeId);
                    if (college != null && college.Status == CollegeStatus.Approved)
                    {
                        job.TargetColleges.Add(new JobTargetCollege
                        {
                            JobId = job.JobId,
                            CollegeId = collegeId
                        });
                    }
                }
            }

            await _jobRepository.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Job created successfully with ID: {JobId} in Draft status", job.JobId);

            var createdJob = await _jobRepository.GetByIdWithDetailsAsync(job.JobId);
            return ApiResponse<JobResponseDto>.SuccessResult(MapToResponseDto(createdJob!), "Job opening created as Draft successfully.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Failed to create job opening.");
            throw;
        }
    }

    public async Task<ApiResponse<JobResponseDto>> UpdateJobAsync(int id, UpdateJobRequestDto request, string hrUserId, string currentUserRole)
    {
        var job = await _jobRepository.GetByIdWithDetailsAsync(id);
        if (job == null)
        {
            return ApiResponse<JobResponseDto>.FailureResult("Job opening not found.");
        }

        if (currentUserRole != AppRoles.SuperAdmin)
        {
            var hr = await _hrRepository.GetByUserIdAsync(hrUserId);
            if (hr == null || hr.CompanyId != job.CompanyId)
            {
                return ApiResponse<JobResponseDto>.FailureResult("You can only modify job openings created for your company.");
            }
        }

        if (job.Status == JobStatus.Closed || job.Status == JobStatus.Cancelled)
        {
            return ApiResponse<JobResponseDto>.FailureResult("Cannot modify a closed or cancelled job opening.");
        }

        job.JobTitle = request.JobTitle.Trim();
        job.JobDescription = request.JobDescription.Trim();
        job.JobType = request.JobType.Trim();
        job.Location = request.Location.Trim();
        job.SalaryPackage = request.SalaryPackage;
        job.MinimumCGPA = request.MinimumCGPA;
        job.MaximumBacklogs = request.MaximumBacklogs;
        job.ApplicationDeadline = request.ApplicationDeadline;
        job.IsOpenToAllColleges = request.IsOpenToAllColleges;

        // Update departments
        _context.JobEligibleDepartments.RemoveRange(job.EligibleDepartments);
        foreach (var dept in request.EligibleDepartments.Distinct())
        {
            job.EligibleDepartments.Add(new JobEligibleDepartment { JobId = job.JobId, Department = dept.Trim() });
        }

        // Update graduation years
        _context.JobEligibleGraduationYears.RemoveRange(job.EligibleGraduationYears);
        foreach (var yr in request.EligibleGraduationYears.Distinct())
        {
            job.EligibleGraduationYears.Add(new JobEligibleGraduationYear { JobId = job.JobId, GraduationYear = yr });
        }

        await _jobRepository.UpdateAsync(job);
        _logger.LogInformation("Job ID {JobId} updated", id);

        var updatedJob = await _jobRepository.GetByIdWithDetailsAsync(id);
        return ApiResponse<JobResponseDto>.SuccessResult(MapToResponseDto(updatedJob!), "Job opening updated successfully.");
    }

    public async Task<ApiResponse<bool>> PublishJobAsync(int id, string hrUserId, string currentUserRole)
    {
        var job = await _jobRepository.GetByIdWithDetailsAsync(id);
        if (job == null)
        {
            return ApiResponse<bool>.FailureResult("Job opening not found.");
        }

        if (currentUserRole != AppRoles.SuperAdmin)
        {
            var hr = await _hrRepository.GetByUserIdAsync(hrUserId);
            if (hr == null || hr.CompanyId != job.CompanyId)
            {
                return ApiResponse<bool>.FailureResult("You can only publish jobs belonging to your company.");
            }
        }

        if (job.Status != JobStatus.Draft)
        {
            return ApiResponse<bool>.FailureResult($"Only Draft jobs can be published. Current status is {job.Status}.");
        }

        if (job.ApplicationDeadline <= DateTime.UtcNow)
        {
            return ApiResponse<bool>.FailureResult("Cannot publish job: Application deadline must be in the future.");
        }

        if (job.Company.Status != CompanyStatus.Approved)
        {
            return ApiResponse<bool>.FailureResult("Cannot publish job: Owning company is not active or approved.");
        }

        job.Status = JobStatus.Published;
        await _jobRepository.UpdateAsync(job);
        _logger.LogInformation("Job ID {JobId} published successfully", id);

        return ApiResponse<bool>.SuccessResult(true, "Job opening published successfully. Students can now view and apply.");
    }

    public async Task<ApiResponse<bool>> CloseJobAsync(int id, string hrUserId, string currentUserRole)
    {
        var job = await _jobRepository.GetByIdAsync(id);
        if (job == null)
        {
            return ApiResponse<bool>.FailureResult("Job opening not found.");
        }

        if (currentUserRole != AppRoles.SuperAdmin)
        {
            var hr = await _hrRepository.GetByUserIdAsync(hrUserId);
            if (hr == null || hr.CompanyId != job.CompanyId)
            {
                return ApiResponse<bool>.FailureResult("You can only close jobs belonging to your company.");
            }
        }

        job.Status = JobStatus.Closed;
        await _jobRepository.UpdateAsync(job);
        _logger.LogInformation("Job ID {JobId} closed", id);

        return ApiResponse<bool>.SuccessResult(true, "Job opening closed successfully.");
    }

    public async Task<ApiResponse<bool>> CancelJobAsync(int id, string hrUserId, string currentUserRole)
    {
        var job = await _jobRepository.GetByIdAsync(id);
        if (job == null)
        {
            return ApiResponse<bool>.FailureResult("Job opening not found.");
        }

        if (currentUserRole != AppRoles.SuperAdmin)
        {
            var hr = await _hrRepository.GetByUserIdAsync(hrUserId);
            if (hr == null || hr.CompanyId != job.CompanyId)
            {
                return ApiResponse<bool>.FailureResult("You can only cancel jobs belonging to your company.");
            }
        }

        job.Status = JobStatus.Cancelled;
        await _jobRepository.UpdateAsync(job);
        _logger.LogInformation("Job ID {JobId} cancelled", id);

        return ApiResponse<bool>.SuccessResult(true, "Job opening cancelled.");
    }

    public async Task<ApiResponse<bool>> DeleteDraftJobAsync(int id, string hrUserId, string currentUserRole)
    {
        var job = await _jobRepository.GetByIdWithDetailsAsync(id);
        if (job == null)
        {
            return ApiResponse<bool>.FailureResult("Job opening not found.");
        }

        if (currentUserRole != AppRoles.SuperAdmin)
        {
            var hr = await _hrRepository.GetByUserIdAsync(hrUserId);
            if (hr == null || hr.CompanyId != job.CompanyId)
            {
                return ApiResponse<bool>.FailureResult("You can only delete jobs belonging to your company.");
            }
        }

        if (job.Status != JobStatus.Draft)
        {
            return ApiResponse<bool>.FailureResult("Only draft jobs with no application history can be deleted.");
        }

        await _jobRepository.DeleteAsync(job);
        _logger.LogInformation("Draft Job ID {JobId} deleted", id);

        return ApiResponse<bool>.SuccessResult(true, "Draft job deleted successfully.");
    }

    public async Task<ApiResponse<JobResponseDto>> GetJobByIdAsync(int id, string currentUserId, string currentUserRole)
    {
        var job = await _jobRepository.GetByIdWithDetailsAsync(id);
        if (job == null)
        {
            return ApiResponse<JobResponseDto>.FailureResult("Job opening not found.");
        }

        return ApiResponse<JobResponseDto>.SuccessResult(MapToResponseDto(job));
    }

    public async Task<ApiResponse<PagedResult<JobResponseDto>>> GetAllVisibleJobsAsync(JobFilterParams filterParams, string currentUserId, string currentUserRole)
    {
        int? targetedCollegeId = null;

        if (currentUserRole == AppRoles.CompanyHR)
        {
            var hr = await _hrRepository.GetByUserIdAsync(currentUserId);
            if (hr != null)
            {
                filterParams.CompanyId = hr.CompanyId;
            }
        }
        else if (currentUserRole == AppRoles.PlacementOfficer)
        {
            var officer = await _context.PlacementOfficers.FirstOrDefaultAsync(po => po.UserId == currentUserId);
            if (officer != null)
            {
                targetedCollegeId = officer.CollegeId;
                filterParams.Status ??= JobStatus.Published;
            }
        }

        var result = await _jobRepository.GetAllAsync(filterParams, targetedCollegeId);
        return ApiResponse<PagedResult<JobResponseDto>>.SuccessResult(result);
    }

    public async Task<ApiResponse<PagedResult<JobResponseDto>>> GetEligibleJobsForStudentAsync(string studentUserId, PaginationParams paginationParams)
    {
        var student = await _studentRepository.GetByUserIdAsync(studentUserId);
        if (student == null || student.Status != StudentStatus.Approved)
        {
            return ApiResponse<PagedResult<JobResponseDto>>.FailureResult("Student is not approved or does not exist.");
        }

        if (student.College.Status != CollegeStatus.Approved)
        {
            return ApiResponse<PagedResult<JobResponseDto>>.FailureResult("Student's college is not active or approved.");
        }

        var publishedJobs = await _jobRepository.GetPublishedJobsWithDetailsAsync();

        var eligibleJobs = publishedJobs
            .Where(j => EvaluateEligibility(j, student).IsEligible)
            .Select(MapToResponseDto)
            .ToList();

        var totalCount = eligibleJobs.Count;
        var pagedItems = eligibleJobs
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToList();

        var pagedResult = new PagedResult<JobResponseDto>(pagedItems, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        return ApiResponse<PagedResult<JobResponseDto>>.SuccessResult(pagedResult);
    }

    public async Task<ApiResponse<JobEligibilityCheckResultDto>> CheckStudentEligibilityAsync(int jobId, string studentUserId)
    {
        var student = await _studentRepository.GetByUserIdAsync(studentUserId);
        if (student == null)
        {
            return ApiResponse<JobEligibilityCheckResultDto>.FailureResult("Student not found.");
        }

        var job = await _jobRepository.GetByIdWithDetailsAsync(jobId);
        if (job == null)
        {
            return ApiResponse<JobEligibilityCheckResultDto>.FailureResult("Job opening not found.");
        }

        var checkResult = EvaluateEligibility(job, student);
        return ApiResponse<JobEligibilityCheckResultDto>.SuccessResult(checkResult);
    }

    public async Task<ApiResponse<bool>> AddTargetCollegesAsync(int jobId, AddTargetCollegesRequestDto request, string hrUserId, string currentUserRole)
    {
        var job = await _jobRepository.GetByIdWithDetailsAsync(jobId);
        if (job == null)
        {
            return ApiResponse<bool>.FailureResult("Job opening not found.");
        }

        if (currentUserRole != AppRoles.SuperAdmin)
        {
            var hr = await _hrRepository.GetByUserIdAsync(hrUserId);
            if (hr == null || hr.CompanyId != job.CompanyId)
            {
                return ApiResponse<bool>.FailureResult("You can only modify college targeting for your company's jobs.");
            }
        }

        foreach (var collegeId in request.CollegeIds.Distinct())
        {
            var college = await _collegeRepository.GetByIdAsync(collegeId);
            if (college != null && college.Status == CollegeStatus.Approved)
            {
                if (!job.TargetColleges.Any(tc => tc.CollegeId == collegeId))
                {
                    await _jobRepository.AddTargetCollegeAsync(new JobTargetCollege
                    {
                        JobId = jobId,
                        CollegeId = collegeId
                    });
                }
            }
        }

        return ApiResponse<bool>.SuccessResult(true, "Target colleges added successfully.");
    }

    public async Task<ApiResponse<bool>> RemoveTargetCollegeAsync(int jobId, int collegeId, string hrUserId, string currentUserRole)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null)
        {
            return ApiResponse<bool>.FailureResult("Job opening not found.");
        }

        if (currentUserRole != AppRoles.SuperAdmin)
        {
            var hr = await _hrRepository.GetByUserIdAsync(hrUserId);
            if (hr == null || hr.CompanyId != job.CompanyId)
            {
                return ApiResponse<bool>.FailureResult("You can only modify college targeting for your company's jobs.");
            }
        }

        await _jobRepository.RemoveTargetCollegeAsync(jobId, collegeId);
        return ApiResponse<bool>.SuccessResult(true, "Target college removed successfully.");
    }

    // Pure reusable eligibility evaluation rule engine
    public static JobEligibilityCheckResultDto EvaluateEligibility(JobOpening job, Student student)
    {
        var failedCriteria = new List<string>();

        if (job.Status != JobStatus.Published)
        {
            failedCriteria.Add($"Job is not published (Status: {job.Status}).");
        }

        if (job.ApplicationDeadline <= DateTime.UtcNow)
        {
            failedCriteria.Add("Application deadline has passed.");
        }

        if (student.Status != StudentStatus.Approved || student.User.AccountStatus != AccountStatus.Active)
        {
            failedCriteria.Add("Student account is not approved and active.");
        }

        if (student.College.Status != CollegeStatus.Approved)
        {
            failedCriteria.Add("Student's college is not approved and active.");
        }

        // College targeting check
        if (!job.IsOpenToAllColleges && !job.TargetColleges.Any(tc => tc.CollegeId == student.CollegeId))
        {
            failedCriteria.Add($"Your college '{student.College.CollegeName}' is not targeted for this job opening.");
        }

        // Department check
        if (job.EligibleDepartments.Any() && !job.EligibleDepartments.Any(d => d.Department.Equals(student.Department, StringComparison.OrdinalIgnoreCase)))
        {
            failedCriteria.Add($"Your department '{student.Department}' is not eligible. Allowed: {string.Join(", ", job.EligibleDepartments.Select(d => d.Department))}.");
        }

        // Graduation Year check
        if (job.EligibleGraduationYears.Any() && !job.EligibleGraduationYears.Any(y => y.GraduationYear == student.GraduationYear))
        {
            failedCriteria.Add($"Your graduation year '{student.GraduationYear}' is not eligible. Allowed: {string.Join(", ", job.EligibleGraduationYears.Select(y => y.GraduationYear))}.");
        }

        // CGPA check
        if (student.CGPA < job.MinimumCGPA)
        {
            failedCriteria.Add($"Your CGPA ({student.CGPA:F2}) does not meet the minimum required CGPA ({job.MinimumCGPA:F2}).");
        }

        // Backlogs check
        if (student.ActiveBacklogs > job.MaximumBacklogs)
        {
            failedCriteria.Add($"Your active backlogs ({student.ActiveBacklogs}) exceed the maximum allowed ({job.MaximumBacklogs}).");
        }

        var isEligible = !failedCriteria.Any();
        var summary = isEligible ? "You satisfy all eligibility requirements for this job." : "You do not meet one or more eligibility criteria.";

        return new JobEligibilityCheckResultDto
        {
            JobId = job.JobId,
            JobTitle = job.JobTitle,
            IsEligible = isEligible,
            Summary = summary,
            FailedCriteria = failedCriteria
        };
    }

    private static JobResponseDto MapToResponseDto(JobOpening j)
    {
        return new JobResponseDto
        {
            JobId = j.JobId,
            CompanyId = j.CompanyId,
            CompanyName = j.Company?.CompanyName ?? string.Empty,
            CreatedByHRId = j.CreatedByHRId,
            HRName = j.CreatedByHR?.User?.FullName ?? string.Empty,
            JobTitle = j.JobTitle,
            JobDescription = j.JobDescription,
            JobType = j.JobType,
            Location = j.Location,
            SalaryPackage = j.SalaryPackage,
            MinimumCGPA = j.MinimumCGPA,
            MaximumBacklogs = j.MaximumBacklogs,
            ApplicationDeadline = j.ApplicationDeadline,
            IsOpenToAllColleges = j.IsOpenToAllColleges,
            Status = j.Status,
            CreatedAt = j.CreatedAt,
            EligibleDepartments = j.EligibleDepartments?.Select(d => d.Department).ToList() ?? new(),
            EligibleGraduationYears = j.EligibleGraduationYears?.Select(y => y.GraduationYear).ToList() ?? new(),
            TargetColleges = j.TargetColleges?.Select(tc => new CollegeResponseDto
            {
                CollegeId = tc.College.CollegeId,
                CollegeName = tc.College.CollegeName,
                CollegeCode = tc.College.CollegeCode,
                UniversityName = tc.College.UniversityName,
                OfficialEmail = tc.College.OfficialEmail,
                PhoneNumber = tc.College.PhoneNumber,
                Address = tc.College.Address,
                Status = tc.College.Status
            }).ToList() ?? new(),
            TotalApplications = j.Applications?.Count ?? 0
        };
    }
}
