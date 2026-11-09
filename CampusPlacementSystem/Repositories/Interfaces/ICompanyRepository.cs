using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Company;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;

namespace CampusPlacementSystem.Repositories.Interfaces;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(int id);
    Task<bool> CompanyEmailExistsAsync(string email, int? excludeId = null);
    Task<bool> CompanyNameExistsAsync(string name, int? excludeId = null);
    Task<PagedResult<CompanyResponseDto>> GetAllAsync(PaginationParams paginationParams, CompanyStatus? status = null);
    Task<Company> AddAsync(Company company);
    Task UpdateAsync(Company company);
    Task<bool> ExistsAsync(int id);
    Task SaveChangesAsync();
}
