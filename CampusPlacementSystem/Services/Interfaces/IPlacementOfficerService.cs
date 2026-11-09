using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.PlacementOfficer;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.Services.Interfaces;

public interface IPlacementOfficerService
{
    Task<ApiResponse<OfficerResponseDto>> RegisterOfficerAsync(RegisterOfficerRequestDto request);
    Task<ApiResponse<PagedResult<OfficerResponseDto>>> GetAllOfficersAsync(PaginationParams paginationParams, OfficerStatus? status = null, int? collegeId = null);
    Task<ApiResponse<PagedResult<OfficerResponseDto>>> GetPendingOfficersAsync(PaginationParams paginationParams);
    Task<ApiResponse<OfficerResponseDto>> GetOfficerByIdAsync(int id);
    Task<ApiResponse<OfficerResponseDto>> GetCurrentOfficerProfileAsync(string userId);
    Task<ApiResponse<OfficerResponseDto>> UpdateOfficerProfileAsync(string userId, UpdateOfficerProfileRequestDto request);
    Task<ApiResponse<bool>> ApproveOfficerAsync(int id, string approvedByUserId);
    Task<ApiResponse<bool>> RejectOfficerAsync(int id);
    Task<ApiResponse<bool>> SuspendOfficerAsync(int id);
}
