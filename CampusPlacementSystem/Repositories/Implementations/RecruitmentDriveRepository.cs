using CampusPlacementSystem.Data;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.RecruitmentDrive;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementSystem.Repositories.Implementations;

public class RecruitmentDriveRepository : IRecruitmentDriveRepository
{
    private readonly AppDbContext _context;

    public RecruitmentDriveRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RecruitmentDrive?> GetByIdAsync(int id)
    {
        return await _context.RecruitmentDrives
            .Include(rd => rd.JobOpening)
                .ThenInclude(j => j.Company)
            .Include(rd => rd.College)
            .FirstOrDefaultAsync(rd => rd.DriveId == id);
    }

    public async Task<RecruitmentDrive?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.RecruitmentDrives
            .Include(rd => rd.JobOpening)
                .ThenInclude(j => j.Company)
            .Include(rd => rd.College)
            .FirstOrDefaultAsync(rd => rd.DriveId == id);
    }

    public async Task<PagedResult<RecruitmentDriveResponseDto>> GetDrivesAsync(
        RecruitmentDriveFilterParams filterParams,
        int? companyId = null,
        int? collegeId = null)
    {
        var query = _context.RecruitmentDrives
            .Include(rd => rd.JobOpening)
                .ThenInclude(j => j.Company)
            .Include(rd => rd.College)
            .AsNoTracking()
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(rd => rd.JobOpening.CompanyId == companyId.Value);
        }

        if (collegeId.HasValue)
        {
            query = query.Where(rd => rd.CollegeId == collegeId.Value);
        }

        if (filterParams.JobId.HasValue)
        {
            query = query.Where(rd => rd.JobId == filterParams.JobId.Value);
        }

        if (filterParams.CollegeId.HasValue)
        {
            query = query.Where(rd => rd.CollegeId == filterParams.CollegeId.Value);
        }

        if (filterParams.Status.HasValue)
        {
            query = query.Where(rd => rd.Status == filterParams.Status.Value);
        }

        if (filterParams.FromDate.HasValue)
        {
            query = query.Where(rd => rd.DriveDate >= filterParams.FromDate.Value);
        }

        if (filterParams.ToDate.HasValue)
        {
            query = query.Where(rd => rd.DriveDate <= filterParams.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Search))
        {
            var search = filterParams.Search.Trim().ToLower();
            query = query.Where(rd => rd.JobOpening.JobTitle.ToLower().Contains(search)
                                   || rd.JobOpening.Company.CompanyName.ToLower().Contains(search)
                                   || rd.College.CollegeName.ToLower().Contains(search)
                                   || rd.College.CollegeCode.ToLower().Contains(search)
                                   || rd.Venue.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(rd => rd.DriveDate)
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Select(rd => new RecruitmentDriveResponseDto
            {
                DriveId = rd.DriveId,
                JobId = rd.JobId,
                JobTitle = rd.JobOpening.JobTitle,
                CompanyId = rd.JobOpening.CompanyId,
                CompanyName = rd.JobOpening.Company.CompanyName,
                CollegeId = rd.CollegeId,
                CollegeName = rd.College.CollegeName,
                CollegeCode = rd.College.CollegeCode,
                DriveDate = rd.DriveDate,
                Venue = rd.Venue,
                Description = rd.Description,
                Status = rd.Status,
                CreatedAt = rd.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<RecruitmentDriveResponseDto>(items, totalCount, filterParams.PageNumber, filterParams.PageSize);
    }

    public async Task<RecruitmentDrive> AddAsync(RecruitmentDrive drive)
    {
        await _context.RecruitmentDrives.AddAsync(drive);
        await _context.SaveChangesAsync();
        return drive;
    }

    public async Task UpdateAsync(RecruitmentDrive drive)
    {
        _context.RecruitmentDrives.Update(drive);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
