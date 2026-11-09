using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.PlacementOfficer;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;

namespace CampusPlacementSystem.Repositories.Interfaces;

public interface IPlacementOfficerRepository
{
    Task<PlacementOfficer?> GetByIdAsync(int id);
    Task<PlacementOfficer?> GetByUserIdAsync(string userId);
    Task<PagedResult<OfficerResponseDto>> GetAllAsync(PaginationParams paginationParams, OfficerStatus? status = null, int? collegeId = null);
    Task<PlacementOfficer> AddAsync(PlacementOfficer officer);
    Task UpdateAsync(PlacementOfficer officer);
    Task<bool> ExistsAsync(int id);
    Task SaveChangesAsync();
}
