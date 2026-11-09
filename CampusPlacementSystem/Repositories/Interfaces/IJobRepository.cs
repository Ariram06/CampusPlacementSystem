using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Job;
using CampusPlacementSystem.Models;

namespace CampusPlacementSystem.Repositories.Interfaces;

public interface IJobRepository
{
    Task<JobOpening?> GetByIdAsync(int id);
    Task<JobOpening?> GetByIdWithDetailsAsync(int id);
    Task<PagedResult<JobResponseDto>> GetAllAsync(JobFilterParams filterParams, int? targetedCollegeId = null);
    Task<List<JobOpening>> GetPublishedJobsWithDetailsAsync();
    Task<JobOpening> AddAsync(JobOpening job);
    Task UpdateAsync(JobOpening job);
    Task DeleteAsync(JobOpening job);
    Task AddTargetCollegeAsync(JobTargetCollege targetCollege);
    Task RemoveTargetCollegeAsync(int jobId, int collegeId);
    Task<List<College>> GetTargetCollegesAsync(int jobId);
    Task<bool> IsCollegeTargetedAsync(int jobId, int collegeId);
    Task SaveChangesAsync();
}
