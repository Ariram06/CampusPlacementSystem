using CampusPlacementSystem.Data;
using CampusPlacementSystem.DTOs.Reports;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementSystem.Repositories.Implementations;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PlatformSummaryReportDto> GetPlatformSummaryAsync()
    {
        var totalColleges = await _context.Colleges.CountAsync();
        var approvedColleges = await _context.Colleges.CountAsync(c => c.Status == CollegeStatus.Approved);
        var pendingColleges = await _context.Colleges.CountAsync(c => c.Status == CollegeStatus.Pending);

        var totalCompanies = await _context.Companies.CountAsync();
        var approvedCompanies = await _context.Companies.CountAsync(c => c.Status == CompanyStatus.Approved);

        var totalStudents = await _context.Students.CountAsync();
        var approvedStudents = await _context.Students.CountAsync(s => s.Status == StudentStatus.Approved);

        var placedStudents = await _context.JobApplications
            .Where(a => a.Status == ApplicationStatus.Selected)
            .Select(a => a.StudentId)
            .Distinct()
            .CountAsync();

        var overallPlacementRate = approvedStudents > 0
            ? Math.Round((decimal)placedStudents * 100m / approvedStudents, 2)
            : 0m;

        var totalJobs = await _context.JobOpenings.CountAsync();
        var publishedJobs = await _context.JobOpenings.CountAsync(j => j.Status == JobStatus.Published);
        var totalApplications = await _context.JobApplications.CountAsync();
        var totalDrives = await _context.RecruitmentDrives.CountAsync();
        var totalInterviews = await _context.InterviewSchedules.CountAsync(s => s.Status == InterviewStatus.Completed);

        return new PlatformSummaryReportDto
        {
            TotalColleges = totalColleges,
            ApprovedColleges = approvedColleges,
            PendingColleges = pendingColleges,
            TotalCompanies = totalCompanies,
            ApprovedCompanies = approvedCompanies,
            TotalStudents = totalStudents,
            ApprovedStudents = approvedStudents,
            PlacedStudents = placedStudents,
            OverallPlacementRate = overallPlacementRate,
            TotalJobOpenings = totalJobs,
            PublishedJobOpenings = publishedJobs,
            TotalApplications = totalApplications,
            TotalRecruitmentDrives = totalDrives,
            TotalInterviewsConducted = totalInterviews
        };
    }

    public async Task<CollegePlacementReportDto?> GetCollegePlacementReportAsync(int collegeId)
    {
        var college = await _context.Colleges
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CollegeId == collegeId);

        if (college == null)
        {
            return null;
        }

        var enrolledStudents = await _context.Students
            .Where(s => s.CollegeId == collegeId)
            .AsNoTracking()
            .ToListAsync();

        var totalEnrolled = enrolledStudents.Count;
        var approvedStudents = enrolledStudents.Count(s => s.Status == StudentStatus.Approved);

        // Selected applications for students of this college
        var selectedApplications = await _context.JobApplications
            .Include(a => a.JobOpening)
                .ThenInclude(j => j.Company)
            .Where(a => a.Student.CollegeId == collegeId && a.Status == ApplicationStatus.Selected)
            .AsNoTracking()
            .ToListAsync();

        var placedStudentIds = selectedApplications.Select(a => a.StudentId).Distinct().ToHashSet();
        var placedCount = placedStudentIds.Count;

        var placementPercentage = approvedStudents > 0
            ? Math.Round((decimal)placedCount * 100m / approvedStudents, 2)
            : 0m;

        var totalApplications = await _context.JobApplications
            .CountAsync(a => a.Student.CollegeId == collegeId);

        var highestPackage = selectedApplications.Any()
            ? selectedApplications.Max(a => a.JobOpening.SalaryPackage)
            : 0m;

        var avgPackage = selectedApplications.Any()
            ? Math.Round(selectedApplications.Average(a => a.JobOpening.SalaryPackage), 2)
            : 0m;

        var totalDrives = await _context.RecruitmentDrives
            .CountAsync(d => d.CollegeId == collegeId);

        // Department Breakdown
        var departmentGroups = enrolledStudents
            .GroupBy(s => s.Department)
            .Select(g =>
            {
                var deptTotal = g.Count();
                var deptPlaced = g.Count(s => placedStudentIds.Contains(s.StudentId));
                var deptRate = deptTotal > 0 ? Math.Round((decimal)deptPlaced * 100m / deptTotal, 2) : 0m;

                return new DepartmentPlacementSummaryDto
                {
                    Department = g.Key,
                    TotalStudents = deptTotal,
                    PlacedStudents = deptPlaced,
                    PlacementRate = deptRate
                };
            })
            .OrderByDescending(d => d.PlacedStudents)
            .ToList();

        // Top Recruiters
        var topRecruiters = selectedApplications
            .GroupBy(a => new { a.JobOpening.CompanyId, a.JobOpening.Company.CompanyName })
            .Select(g => new TopRecruiterDto
            {
                CompanyId = g.Key.CompanyId,
                CompanyName = g.Key.CompanyName,
                SelectedCount = g.Count(),
                HighestPackage = g.Max(a => a.JobOpening.SalaryPackage)
            })
            .OrderByDescending(r => r.SelectedCount)
            .Take(10)
            .ToList();

        return new CollegePlacementReportDto
        {
            CollegeId = college.CollegeId,
            CollegeName = college.CollegeName,
            CollegeCode = college.CollegeCode,
            TotalEnrolledStudents = totalEnrolled,
            ApprovedStudents = approvedStudents,
            PlacedStudents = placedCount,
            PlacementPercentage = placementPercentage,
            TotalApplicationsSubmitted = totalApplications,
            HighestSalaryPackage = highestPackage,
            AverageSalaryPackage = avgPackage,
            TotalRecruitmentDrives = totalDrives,
            DepartmentBreakdown = departmentGroups,
            TopRecruiters = topRecruiters
        };
    }

    public async Task<CompanyRecruitmentReportDto?> GetCompanyRecruitmentReportAsync(int companyId)
    {
        var company = await _context.Companies
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CompanyId == companyId);

        if (company == null)
        {
            return null;
        }

        var jobs = await _context.JobOpenings
            .Include(j => j.Applications)
            .Where(j => j.CompanyId == companyId)
            .AsNoTracking()
            .ToListAsync();

        var totalJobs = jobs.Count;
        var publishedJobs = jobs.Count(j => j.Status == JobStatus.Published);

        var allApplications = jobs.SelectMany(j => j.Applications).ToList();
        var totalApplications = allApplications.Count;
        var underReview = allApplications.Count(a => a.Status == ApplicationStatus.UnderReview);
        var shortlisted = allApplications.Count(a => a.Status == ApplicationStatus.Shortlisted);
        var selected = allApplications.Count(a => a.Status == ApplicationStatus.Selected);

        var jobIds = jobs.Select(j => j.JobId).ToList();

        var totalInterviews = await _context.InterviewSchedules
            .CountAsync(s => jobIds.Contains(s.InterviewRound.JobId));

        var completedInterviews = await _context.InterviewSchedules
            .CountAsync(s => jobIds.Contains(s.InterviewRound.JobId) && s.Status == InterviewStatus.Completed);

        var totalDrives = await _context.RecruitmentDrives
            .CountAsync(d => jobIds.Contains(d.JobId));

        var jobBreakdown = jobs.Select(j => new JobHiringSummaryDto
        {
            JobId = j.JobId,
            JobTitle = j.JobTitle,
            Status = j.Status.ToString(),
            SalaryPackage = j.SalaryPackage,
            TotalApplications = j.Applications.Count,
            Shortlisted = j.Applications.Count(a => a.Status == ApplicationStatus.Shortlisted),
            Selected = j.Applications.Count(a => a.Status == ApplicationStatus.Selected)
        }).ToList();

        return new CompanyRecruitmentReportDto
        {
            CompanyId = company.CompanyId,
            CompanyName = company.CompanyName,
            TotalJobsPosted = totalJobs,
            PublishedJobs = publishedJobs,
            TotalApplicationsReceived = totalApplications,
            UnderReviewApplications = underReview,
            ShortlistedCandidates = shortlisted,
            SelectedCandidates = selected,
            TotalInterviewsScheduled = totalInterviews,
            TotalInterviewsCompleted = completedInterviews,
            TotalRecruitmentDrives = totalDrives,
            JobBreakdown = jobBreakdown
        };
    }

    public async Task<StudentPlacementStatusDto?> GetStudentPlacementReportAsync(int studentId)
    {
        var student = await _context.Students
            .Include(s => s.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentId == studentId);

        if (student == null)
        {
            return null;
        }

        var applications = await _context.JobApplications
            .Include(a => a.JobOpening)
                .ThenInclude(j => j.Company)
            .Where(a => a.StudentId == studentId)
            .AsNoTracking()
            .ToListAsync();

        var totalSubmitted = applications.Count;
        var underReview = applications.Count(a => a.Status == ApplicationStatus.UnderReview);
        var shortlisted = applications.Count(a => a.Status == ApplicationStatus.Shortlisted);
        var interviewScheduled = applications.Count(a => a.Status == ApplicationStatus.InterviewScheduled);

        var selectedApps = applications.Where(a => a.Status == ApplicationStatus.Selected).ToList();
        var selectedCount = selectedApps.Count;

        var offers = selectedApps.Select(a => new StudentOfferDto
        {
            ApplicationId = a.ApplicationId,
            JobId = a.JobId,
            JobTitle = a.JobOpening.JobTitle,
            CompanyName = a.JobOpening.Company?.CompanyName ?? string.Empty,
            SalaryPackage = a.JobOpening.SalaryPackage,
            SelectedAt = a.LastUpdatedAt ?? a.AppliedAt
        }).ToList();

        return new StudentPlacementStatusDto
        {
            StudentId = student.StudentId,
            FullName = student.User?.FullName ?? string.Empty,
            RegisterNumber = student.RegisterNumber,
            Department = student.Department,
            Degree = student.Degree,
            TotalApplicationsSubmitted = totalSubmitted,
            UnderReviewApplications = underReview,
            ShortlistedApplications = shortlisted,
            InterviewsScheduledCount = interviewScheduled,
            SelectedOffersCount = selectedCount,
            OffersReceived = offers
        };
    }
}
