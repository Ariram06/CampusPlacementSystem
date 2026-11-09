using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.CompanyHR;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;

namespace CampusPlacementSystem.Repositories.Interfaces;

public interface ICompanyHRRepository
{
    Task<CompanyHR?> GetByIdAsync(int id);
    Task<CompanyHR?> GetByUserIdAsync(string userId);
    Task<PagedResult<CompanyHRResponseDto>> GetAllAsync(PaginationParams paginationParams, AccountStatus? status = null, int? companyId = null);
    Task<CompanyHR> AddAsync(CompanyHR companyHR);
    Task UpdateAsync(CompanyHR companyHR);
    Task<bool> ExistsAsync(int id);
    Task SaveChangesAsync();
}
