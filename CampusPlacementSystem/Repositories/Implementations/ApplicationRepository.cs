using CampusPlacementSystem.Data;
using CampusPlacementSystem.DTOs.Application;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementSystem.Repositories.Implementations;

public class ApplicationRepository : IApplicationRepository
{
    private readonly AppDbContext _context;

    public ApplicationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<JobApplication?> GetByIdAsync(int id)
    {
        return await _context.JobApplications
            .Include(a => a.JobOpening)
                .ThenInclude(j => j.Company)
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Student)
                .ThenInclude(s => s.College)
            .FirstOrDefaultAsync(a => a.ApplicationId == id);
    }

    public async Task<JobApplication?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.JobApplications
            .Include(a => a.JobOpening)
                .ThenInclude(j => j.Company)
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Student)
                .ThenInclude(s => s.College)
            .Include(a => a.InterviewSchedules)
            .Include(a => a.StatusHistories)
                .ThenInclude(h => h.UpdatedByUser)
            .FirstOrDefaultAsync(a => a.ApplicationId == id);
    }

    public async Task<bool> HasStudentAppliedAsync(int studentId, int jobId)
    {
        return await _context.JobApplications
            .AnyAsync(a => a.StudentId == studentId && a.JobId == jobId);
    }

    public async Task<PagedResult<ApplicationResponseDto>> GetApplicationsAsync(
        ApplicationFilterParams filterParams,
        int? companyId = null,
        int? studentId = null,
        int? collegeId = null)
    {
        var query = _context.JobApplications
            .Include(a => a.JobOpening)
                .ThenInclude(j => j.Company)
            .Include(a => a.Student)
                .ThenInclude(s => s.User)
            .Include(a => a.Student)
                .ThenInclude(s => s.College)
            .Include(a => a.InterviewSchedules)
            .AsNoTracking()
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(a => a.JobOpening.CompanyId == companyId.Value);
        }

        if (studentId.HasValue)
        {
            query = query.Where(a => a.StudentId == studentId.Value);
        }

        if (collegeId.HasValue)
        {
            query = query.Where(a => a.Student.CollegeId == collegeId.Value);
        }

        if (filterParams.JobId.HasValue)
        {
            query = query.Where(a => a.JobId == filterParams.JobId.Value);
        }

        if (filterParams.Status.HasValue)
        {
            query = query.Where(a => a.Status == filterParams.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Department))
        {
            query = query.Where(a => a.Student.Department.ToLower() == filterParams.Department.Trim().ToLower());
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Search))
        {
            var search = filterParams.Search.Trim().ToLower();
            query = query.Where(a => a.Student.User.FullName.ToLower().Contains(search)
                                  || a.Student.RegisterNumber.ToLower().Contains(search)
                                  || a.JobOpening.JobTitle.ToLower().Contains(search)
                                  || a.JobOpening.Company.CompanyName.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.AppliedAt)
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Select(a => new ApplicationResponseDto
            {
                ApplicationId = a.ApplicationId,
                JobId = a.JobId,
                JobTitle = a.JobOpening.JobTitle,
                CompanyId = a.JobOpening.CompanyId,
                CompanyName = a.JobOpening.Company.CompanyName,
                StudentId = a.StudentId,
                StudentName = a.Student.User.FullName,
                StudentEmail = a.Student.User.Email ?? string.Empty,
                RegisterNumber = a.Student.RegisterNumber,
                Department = a.Student.Department,
                Degree = a.Student.Degree,
                GraduationYear = a.Student.GraduationYear,
                CGPA = a.Student.CGPA,
                ActiveBacklogs = a.Student.ActiveBacklogs,
                CollegeId = a.Student.CollegeId,
                CollegeName = a.Student.College.CollegeName,
                AppliedAt = a.AppliedAt,
                Status = a.Status,
                ResumeUrl = a.ResumeUrl,
                LastUpdatedAt = a.LastUpdatedAt,
                TotalInterviewSchedules = a.InterviewSchedules.Count
            })
            .ToListAsync();

        return new PagedResult<ApplicationResponseDto>(items, totalCount, filterParams.PageNumber, filterParams.PageSize);
    }

    public async Task<List<ApplicationStatusHistory>> GetStatusHistoryAsync(int applicationId)
    {
        return await _context.ApplicationStatusHistories
            .Include(h => h.UpdatedByUser)
            .Where(h => h.ApplicationId == applicationId)
            .OrderByDescending(h => h.UpdatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<JobApplication> AddAsync(JobApplication application)
    {
        await _context.JobApplications.AddAsync(application);
        await _context.SaveChangesAsync();
        return application;
    }

    public async Task UpdateAsync(JobApplication application)
    {
        _context.JobApplications.Update(application);
        await _context.SaveChangesAsync();
    }

    public async Task AddStatusHistoryAsync(ApplicationStatusHistory history)
    {
        await _context.ApplicationStatusHistories.AddAsync(history);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
