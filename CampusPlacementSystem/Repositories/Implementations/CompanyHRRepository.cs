using CampusPlacementSystem.Data;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.CompanyHR;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementSystem.Repositories.Implementations;

public class CompanyHRRepository : ICompanyHRRepository
{
    private readonly AppDbContext _context;

    public CompanyHRRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CompanyHR?> GetByIdAsync(int id)
    {
        return await _context.CompanyHRs
            .Include(hr => hr.User)
            .Include(hr => hr.Company)
            .Include(hr => hr.CreatedJobOpenings)
            .FirstOrDefaultAsync(hr => hr.CompanyHRId == id);
    }

    public async Task<CompanyHR?> GetByUserIdAsync(string userId)
    {
        return await _context.CompanyHRs
            .Include(hr => hr.User)
            .Include(hr => hr.Company)
            .Include(hr => hr.CreatedJobOpenings)
            .FirstOrDefaultAsync(hr => hr.UserId == userId);
    }

    public async Task<PagedResult<CompanyHRResponseDto>> GetAllAsync(PaginationParams paginationParams, AccountStatus? status = null, int? companyId = null)
    {
        var query = _context.CompanyHRs
            .Include(hr => hr.User)
            .Include(hr => hr.Company)
            .Include(hr => hr.CreatedJobOpenings)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(hr => hr.User.AccountStatus == status.Value);
        }

        if (companyId.HasValue)
        {
            query = query.Where(hr => hr.CompanyId == companyId.Value);
        }

        if (!string.IsNullOrWhiteSpace(paginationParams.Search))
        {
            var search = paginationParams.Search.Trim().ToLower();
            query = query.Where(hr => hr.User.FullName.ToLower().Contains(search)
                                   || (hr.User.Email != null && hr.User.Email.ToLower().Contains(search))
                                   || hr.Company.CompanyName.ToLower().Contains(search)
                                   || hr.Designation.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(hr => hr.CreatedAt)
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .Select(hr => new CompanyHRResponseDto
            {
                CompanyHRId = hr.CompanyHRId,
                UserId = hr.UserId,
                FullName = hr.User.FullName,
                Email = hr.User.Email ?? string.Empty,
                PhoneNumber = hr.User.PhoneNumber,
                CompanyId = hr.CompanyId,
                CompanyName = hr.Company.CompanyName,
                Designation = hr.Designation,
                AccountStatus = hr.User.AccountStatus,
                ApprovedBy = hr.ApprovedBy,
                ApprovedAt = hr.ApprovedAt,
                CreatedAt = hr.CreatedAt,
                TotalJobsPosted = hr.CreatedJobOpenings.Count
            })
            .ToListAsync();

        return new PagedResult<CompanyHRResponseDto>(items, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<CompanyHR> AddAsync(CompanyHR companyHR)
    {
        await _context.CompanyHRs.AddAsync(companyHR);
        await _context.SaveChangesAsync();
        return companyHR;
    }

    public async Task UpdateAsync(CompanyHR companyHR)
    {
        _context.CompanyHRs.Update(companyHR);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.CompanyHRs.AnyAsync(hr => hr.CompanyHRId == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
