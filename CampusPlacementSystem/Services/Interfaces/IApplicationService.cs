using CampusPlacementSystem.DTOs.Application;
using CampusPlacementSystem.DTOs.Common;

namespace CampusPlacementSystem.Services.Interfaces;

public interface IApplicationService
{
    Task<ApiResponse<ApplicationResponseDto>> ApplyJobAsync(ApplyJobRequestDto request, string studentUserId);
    Task<ApiResponse<PagedResult<ApplicationResponseDto>>> GetMyApplicationsAsync(string studentUserId, ApplicationFilterParams filterParams);
    Task<ApiResponse<PagedResult<ApplicationResponseDto>>> GetJobApplicantsAsync(int jobId, ApplicationFilterParams filterParams, string hrUserId, string currentUserRole);
    Task<ApiResponse<ApplicationResponseDto>> GetApplicationByIdAsync(int id, string currentUserId, string currentUserRole);
    Task<ApiResponse<bool>> UpdateApplicationStatusAsync(int id, UpdateApplicationStatusRequestDto request, string hrUserId, string currentUserRole);
    Task<ApiResponse<bool>> WithdrawApplicationAsync(int id, string studentUserId);
    Task<ApiResponse<List<ApplicationStatusHistoryResponseDto>>> GetApplicationStatusHistoryAsync(int id, string currentUserId, string currentUserRole);
}
