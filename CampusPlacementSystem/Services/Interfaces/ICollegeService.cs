using CampusPlacementSystem.DTOs.College;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.Services.Interfaces;

public interface ICollegeService
{
    Task<ApiResponse<CollegeResponseDto>> RegisterCollegeAsync(CreateCollegeRequestDto request);
    Task<ApiResponse<PagedResult<CollegeResponseDto>>> GetAllCollegesAsync(PaginationParams paginationParams, CollegeStatus? status = null);
    Task<ApiResponse<PagedResult<CollegeResponseDto>>> GetPendingCollegesAsync(PaginationParams paginationParams);
    Task<ApiResponse<CollegeResponseDto>> GetCollegeByIdAsync(int collegeId);
    Task<ApiResponse<CollegeResponseDto>> UpdateCollegeAsync(int collegeId, UpdateCollegeRequestDto request, string currentUserId, string currentUserRole);
    Task<ApiResponse<bool>> ApproveCollegeAsync(int collegeId);
    Task<ApiResponse<bool>> RejectCollegeAsync(int collegeId);
    Task<ApiResponse<bool>> SuspendCollegeAsync(int collegeId);
}
