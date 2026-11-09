using CampusPlacementSystem.DTOs.Application;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.Models;

namespace CampusPlacementSystem.Repositories.Interfaces;

public interface IApplicationRepository
{
    Task<JobApplication?> GetByIdAsync(int id);
    Task<JobApplication?> GetByIdWithDetailsAsync(int id);
    Task<bool> HasStudentAppliedAsync(int studentId, int jobId);
    Task<PagedResult<ApplicationResponseDto>> GetApplicationsAsync(ApplicationFilterParams filterParams, int? companyId = null, int? studentId = null, int? collegeId = null);
    Task<List<ApplicationStatusHistory>> GetStatusHistoryAsync(int applicationId);
    Task<JobApplication> AddAsync(JobApplication application);
    Task UpdateAsync(JobApplication application);
    Task AddStatusHistoryAsync(ApplicationStatusHistory history);
    Task SaveChangesAsync();
}
