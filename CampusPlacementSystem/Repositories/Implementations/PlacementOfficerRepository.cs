using CampusPlacementSystem.Data;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.PlacementOfficer;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementSystem.Repositories.Implementations;

public class PlacementOfficerRepository : IPlacementOfficerRepository
{
    private readonly AppDbContext _context;

    public PlacementOfficerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PlacementOfficer?> GetByIdAsync(int id)
    {
        return await _context.PlacementOfficers
            .Include(po => po.User)
            .Include(po => po.College)
            .Include(po => po.EnrolledStudents)
            .FirstOrDefaultAsync(po => po.PlacementOfficerId == id);
    }

    public async Task<PlacementOfficer?> GetByUserIdAsync(string userId)
    {
        return await _context.PlacementOfficers
            .Include(po => po.User)
            .Include(po => po.College)
            .Include(po => po.EnrolledStudents)
            .FirstOrDefaultAsync(po => po.UserId == userId);
    }

    public async Task<PagedResult<OfficerResponseDto>> GetAllAsync(PaginationParams paginationParams, OfficerStatus? status = null, int? collegeId = null)
    {
        var query = _context.PlacementOfficers
            .Include(po => po.User)
            .Include(po => po.College)
            .Include(po => po.EnrolledStudents)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(po => po.Status == status.Value);
        }

        if (collegeId.HasValue)
        {
            query = query.Where(po => po.CollegeId == collegeId.Value);
        }

        if (!string.IsNullOrWhiteSpace(paginationParams.Search))
        {
            var search = paginationParams.Search.Trim().ToLower();
            query = query.Where(po => po.User.FullName.ToLower().Contains(search)
                                   || (po.User.Email != null && po.User.Email.ToLower().Contains(search))
                                   || po.College.CollegeName.ToLower().Contains(search)
                                   || po.Designation.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(po => po.CreatedAt)
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .Select(po => new OfficerResponseDto
            {
                PlacementOfficerId = po.PlacementOfficerId,
                UserId = po.UserId,
                FullName = po.User.FullName,
                Email = po.User.Email ?? string.Empty,
                PhoneNumber = po.User.PhoneNumber,
                CollegeId = po.CollegeId,
                CollegeName = po.College.CollegeName,
                CollegeCode = po.College.CollegeCode,
                Designation = po.Designation,
                Status = po.Status,
                ApprovedBy = po.ApprovedBy,
                ApprovedAt = po.ApprovedAt,
                CreatedAt = po.CreatedAt,
                TotalEnrolledStudents = po.EnrolledStudents.Count
            })
            .ToListAsync();

        return new PagedResult<OfficerResponseDto>(items, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<PlacementOfficer> AddAsync(PlacementOfficer officer)
    {
        await _context.PlacementOfficers.AddAsync(officer);
        await _context.SaveChangesAsync();
        return officer;
    }

    public async Task UpdateAsync(PlacementOfficer officer)
    {
        _context.PlacementOfficers.Update(officer);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.PlacementOfficers.AnyAsync(po => po.PlacementOfficerId == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
