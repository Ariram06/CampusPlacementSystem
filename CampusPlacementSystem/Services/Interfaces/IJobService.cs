using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Job;

namespace CampusPlacementSystem.Services.Interfaces;

public interface IJobService
{
    Task<ApiResponse<JobResponseDto>> CreateJobAsync(CreateJobRequestDto request, string hrUserId);
    Task<ApiResponse<JobResponseDto>> UpdateJobAsync(int id, UpdateJobRequestDto request, string hrUserId, string currentUserRole);
    Task<ApiResponse<bool>> PublishJobAsync(int id, string hrUserId, string currentUserRole);
    Task<ApiResponse<bool>> CloseJobAsync(int id, string hrUserId, string currentUserRole);
    Task<ApiResponse<bool>> CancelJobAsync(int id, string hrUserId, string currentUserRole);
    Task<ApiResponse<bool>> DeleteDraftJobAsync(int id, string hrUserId, string currentUserRole);
    Task<ApiResponse<JobResponseDto>> GetJobByIdAsync(int id, string currentUserId, string currentUserRole);
    Task<ApiResponse<PagedResult<JobResponseDto>>> GetAllVisibleJobsAsync(JobFilterParams filterParams, string currentUserId, string currentUserRole);
    Task<ApiResponse<PagedResult<JobResponseDto>>> GetEligibleJobsForStudentAsync(string studentUserId, PaginationParams paginationParams);
    Task<ApiResponse<JobEligibilityCheckResultDto>> CheckStudentEligibilityAsync(int jobId, string studentUserId);
    Task<ApiResponse<bool>> AddTargetCollegesAsync(int jobId, AddTargetCollegesRequestDto request, string hrUserId, string currentUserRole);
    Task<ApiResponse<bool>> RemoveTargetCollegeAsync(int jobId, int collegeId, string hrUserId, string currentUserRole);
}
