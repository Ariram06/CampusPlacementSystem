using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.RecruitmentDrive;
using CampusPlacementSystem.Models;

namespace CampusPlacementSystem.Repositories.Interfaces;

public interface IRecruitmentDriveRepository
{
    Task<RecruitmentDrive?> GetByIdAsync(int id);
    Task<RecruitmentDrive?> GetByIdWithDetailsAsync(int id);
    Task<PagedResult<RecruitmentDriveResponseDto>> GetDrivesAsync(
        RecruitmentDriveFilterParams filterParams,
        int? companyId = null,
        int? collegeId = null);
    Task<RecruitmentDrive> AddAsync(RecruitmentDrive drive);
    Task UpdateAsync(RecruitmentDrive drive);
    Task SaveChangesAsync();
}
