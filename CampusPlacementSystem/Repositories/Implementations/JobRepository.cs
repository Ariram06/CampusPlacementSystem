using CampusPlacementSystem.Data;
using CampusPlacementSystem.DTOs.College;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Job;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementSystem.Repositories.Implementations;

public class JobRepository : IJobRepository
{
    private readonly AppDbContext _context;

    public JobRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<JobOpening?> GetByIdAsync(int id)
    {
        return await _context.JobOpenings
            .Include(j => j.Company)
            .Include(j => j.CreatedByHR)
                .ThenInclude(hr => hr.User)
            .FirstOrDefaultAsync(j => j.JobId == id);
    }

    public async Task<JobOpening?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.JobOpenings
            .Include(j => j.Company)
            .Include(j => j.CreatedByHR)
                .ThenInclude(hr => hr.User)
            .Include(j => j.EligibleDepartments)
            .Include(j => j.EligibleGraduationYears)
            .Include(j => j.TargetColleges)
                .ThenInclude(tc => tc.College)
            .Include(j => j.Applications)
            .FirstOrDefaultAsync(j => j.JobId == id);
    }

    public async Task<PagedResult<JobResponseDto>> GetAllAsync(JobFilterParams filterParams, int? targetedCollegeId = null)
    {
        var query = _context.JobOpenings
            .Include(j => j.Company)
            .Include(j => j.CreatedByHR)
                .ThenInclude(hr => hr.User)
            .Include(j => j.EligibleDepartments)
            .Include(j => j.EligibleGraduationYears)
            .Include(j => j.TargetColleges)
                .ThenInclude(tc => tc.College)
            .Include(j => j.Applications)
            .AsNoTracking()
            .AsQueryable();

        if (filterParams.Status.HasValue)
        {
            query = query.Where(j => j.Status == filterParams.Status.Value);
        }

        if (filterParams.CompanyId.HasValue)
        {
            query = query.Where(j => j.CompanyId == filterParams.CompanyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.JobType))
        {
            query = query.Where(j => j.JobType.ToLower() == filterParams.JobType.Trim().ToLower());
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Location))
        {
            query = query.Where(j => j.Location.ToLower().Contains(filterParams.Location.Trim().ToLower()));
        }

        if (filterParams.MinSalary.HasValue)
        {
            query = query.Where(j => j.SalaryPackage >= filterParams.MinSalary.Value);
        }

        if (targetedCollegeId.HasValue)
        {
            query = query.Where(j => j.IsOpenToAllColleges || j.TargetColleges.Any(tc => tc.CollegeId == targetedCollegeId.Value));
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Search))
        {
            var search = filterParams.Search.Trim().ToLower();
            query = query.Where(j => j.JobTitle.ToLower().Contains(search)
                                  || j.Company.CompanyName.ToLower().Contains(search)
                                  || j.Location.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(j => j.CreatedAt)
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Select(j => new JobResponseDto
            {
                JobId = j.JobId,
                CompanyId = j.CompanyId,
                CompanyName = j.Company.CompanyName,
                CreatedByHRId = j.CreatedByHRId,
                HRName = j.CreatedByHR.User.FullName,
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
                EligibleDepartments = j.EligibleDepartments.Select(d => d.Department).ToList(),
                EligibleGraduationYears = j.EligibleGraduationYears.Select(y => y.GraduationYear).ToList(),
                TargetColleges = j.TargetColleges.Select(tc => new CollegeResponseDto
                {
                    CollegeId = tc.College.CollegeId,
                    CollegeName = tc.College.CollegeName,
                    CollegeCode = tc.College.CollegeCode,
                    UniversityName = tc.College.UniversityName,
                    OfficialEmail = tc.College.OfficialEmail,
                    PhoneNumber = tc.College.PhoneNumber,
                    Address = tc.College.Address,
                    Status = tc.College.Status
                }).ToList(),
                TotalApplications = j.Applications.Count
            })
            .ToListAsync();

        return new PagedResult<JobResponseDto>(items, totalCount, filterParams.PageNumber, filterParams.PageSize);
    }

    public async Task<List<JobOpening>> GetPublishedJobsWithDetailsAsync()
    {
        return await _context.JobOpenings
            .Include(j => j.Company)
            .Include(j => j.CreatedByHR)
                .ThenInclude(hr => hr.User)
            .Include(j => j.EligibleDepartments)
            .Include(j => j.EligibleGraduationYears)
            .Include(j => j.TargetColleges)
                .ThenInclude(tc => tc.College)
            .Include(j => j.Applications)
            .Where(j => j.Status == Enums.JobStatus.Published && j.ApplicationDeadline > DateTime.UtcNow)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<JobOpening> AddAsync(JobOpening job)
    {
        await _context.JobOpenings.AddAsync(job);
        await _context.SaveChangesAsync();
        return job;
    }

    public async Task UpdateAsync(JobOpening job)
    {
        _context.JobOpenings.Update(job);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(JobOpening job)
    {
        _context.JobOpenings.Remove(job);
        await _context.SaveChangesAsync();
    }

    public async Task AddTargetCollegeAsync(JobTargetCollege targetCollege)
    {
        await _context.JobTargetColleges.AddAsync(targetCollege);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveTargetCollegeAsync(int jobId, int collegeId)
    {
        var target = await _context.JobTargetColleges
            .FirstOrDefaultAsync(tc => tc.JobId == jobId && tc.CollegeId == collegeId);

        if (target != null)
        {
            _context.JobTargetColleges.Remove(target);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<College>> GetTargetCollegesAsync(int jobId)
    {
        return await _context.JobTargetColleges
            .Where(tc => tc.JobId == jobId)
            .Include(tc => tc.College)
            .Select(tc => tc.College)
            .ToListAsync();
    }

    public async Task<bool> IsCollegeTargetedAsync(int jobId, int collegeId)
    {
        return await _context.JobTargetColleges
            .AnyAsync(tc => tc.JobId == jobId && tc.CollegeId == collegeId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
