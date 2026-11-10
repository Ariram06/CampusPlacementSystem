using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.RecruitmentDrive;

namespace CampusPlacementSystem.Services.Interfaces;

public interface IRecruitmentDriveService
{
    Task<ApiResponse<RecruitmentDriveResponseDto>> CreateDriveAsync(CreateRecruitmentDriveRequestDto request, string hrUserId, string currentUserRole);
    Task<ApiResponse<RecruitmentDriveResponseDto>> UpdateDriveAsync(int id, UpdateRecruitmentDriveRequestDto request, string hrUserId, string currentUserRole);
    Task<ApiResponse<RecruitmentDriveResponseDto>> GetDriveByIdAsync(int id, string currentUserId, string currentUserRole);
    Task<ApiResponse<PagedResult<RecruitmentDriveResponseDto>>> GetDrivesAsync(RecruitmentDriveFilterParams filterParams, string currentUserId, string currentUserRole);
    Task<ApiResponse<bool>> CancelDriveAsync(int id, string hrUserId, string currentUserRole);
}

