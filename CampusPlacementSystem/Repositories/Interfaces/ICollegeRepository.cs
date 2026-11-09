using CampusPlacementSystem.DTOs.College;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;

namespace CampusPlacementSystem.Repositories.Interfaces;

public interface ICollegeRepository
{
    Task<College?> GetByIdAsync(int collegeId);
    Task<College?> GetByCodeAsync(string collegeCode);
    Task<bool> CollegeCodeExistsAsync(string collegeCode, int? excludeCollegeId = null);
    Task<bool> OfficialEmailExistsAsync(string email, int? excludeCollegeId = null);
    Task<PagedResult<CollegeResponseDto>> GetAllAsync(PaginationParams paginationParams, CollegeStatus? status = null);
    Task<College> AddAsync(College college);
    Task UpdateAsync(College college);
    Task<bool> ExistsAsync(int collegeId);
    Task SaveChangesAsync();
}
