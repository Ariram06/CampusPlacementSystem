using CampusPlacementSystem.Data;
using CampusPlacementSystem.DTOs.College;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementSystem.Repositories.Implementations;

public class CollegeRepository : ICollegeRepository
{
    private readonly AppDbContext _context;

    public CollegeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<College?> GetByIdAsync(int collegeId)
    {
        return await _context.Colleges
            .Include(c => c.PlacementOfficers)
            .Include(c => c.Students)
            .FirstOrDefaultAsync(c => c.CollegeId == collegeId);
    }

    public async Task<College?> GetByCodeAsync(string collegeCode)
    {
        return await _context.Colleges
            .FirstOrDefaultAsync(c => c.CollegeCode == collegeCode);
    }

    public async Task<bool> CollegeCodeExistsAsync(string collegeCode, int? excludeCollegeId = null)
    {
        return await _context.Colleges
            .AnyAsync(c => c.CollegeCode == collegeCode && (!excludeCollegeId.HasValue || c.CollegeId != excludeCollegeId.Value));
    }

    public async Task<bool> OfficialEmailExistsAsync(string email, int? excludeCollegeId = null)
    {
        return await _context.Colleges
            .AnyAsync(c => c.OfficialEmail == email && (!excludeCollegeId.HasValue || c.CollegeId != excludeCollegeId.Value));
    }

    public async Task<PagedResult<CollegeResponseDto>> GetAllAsync(PaginationParams paginationParams, CollegeStatus? status = null)
    {
        var query = _context.Colleges.AsNoTracking().AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(paginationParams.Search))
        {
            var search = paginationParams.Search.Trim().ToLower();
            query = query.Where(c => c.CollegeName.ToLower().Contains(search)
                                  || c.CollegeCode.ToLower().Contains(search)
                                  || c.UniversityName.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .Select(c => new CollegeResponseDto
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
                TotalPlacementOfficers = c.PlacementOfficers.Count,
                TotalStudents = c.Students.Count
            })
            .ToListAsync();

        return new PagedResult<CollegeResponseDto>(items, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<College> AddAsync(College college)
    {
        await _context.Colleges.AddAsync(college);
        await _context.SaveChangesAsync();
        return college;
    }

    public async Task UpdateAsync(College college)
    {
        _context.Colleges.Update(college);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int collegeId)
    {
        return await _context.Colleges.AnyAsync(c => c.CollegeId == collegeId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
