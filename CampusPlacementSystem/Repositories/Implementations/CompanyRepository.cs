using CampusPlacementSystem.Data;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Company;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementSystem.Repositories.Implementations;

public class CompanyRepository : ICompanyRepository
{
    private readonly AppDbContext _context;

    public CompanyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Company?> GetByIdAsync(int id)
    {
        return await _context.Companies
            .Include(c => c.HRUsers)
            .Include(c => c.JobOpenings)
            .FirstOrDefaultAsync(c => c.CompanyId == id);
    }

    public async Task<bool> CompanyEmailExistsAsync(string email, int? excludeId = null)
    {
        return await _context.Companies
            .AnyAsync(c => c.CompanyEmail == email && (!excludeId.HasValue || c.CompanyId != excludeId.Value));
    }

    public async Task<bool> CompanyNameExistsAsync(string name, int? excludeId = null)
    {
        return await _context.Companies
            .AnyAsync(c => c.CompanyName.ToLower() == name.ToLower() && (!excludeId.HasValue || c.CompanyId != excludeId.Value));
    }

    public async Task<PagedResult<CompanyResponseDto>> GetAllAsync(PaginationParams paginationParams, CompanyStatus? status = null)
    {
        var query = _context.Companies
            .Include(c => c.HRUsers)
            .Include(c => c.JobOpenings)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(c => c.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(paginationParams.Search))
        {
            var search = paginationParams.Search.Trim().ToLower();
            query = query.Where(c => c.CompanyName.ToLower().Contains(search)
                                  || c.CompanyEmail.ToLower().Contains(search)
                                  || c.Industry.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .Select(c => new CompanyResponseDto
            {
                CompanyId = c.CompanyId,
                CompanyName = c.CompanyName,
                CompanyEmail = c.CompanyEmail,
                Website = c.Website,
                Industry = c.Industry,
                Address = c.Address,
                Description = c.Description,
                Status = c.Status,
                CreatedAt = c.CreatedAt,
                TotalHRUsers = c.HRUsers.Count,
                TotalJobOpenings = c.JobOpenings.Count
            })
            .ToListAsync();

        return new PagedResult<CompanyResponseDto>(items, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<Company> AddAsync(Company company)
    {
        await _context.Companies.AddAsync(company);
        await _context.SaveChangesAsync();
        return company;
    }

    public async Task UpdateAsync(Company company)
    {
        _context.Companies.Update(company);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Companies.AnyAsync(c => c.CompanyId == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
