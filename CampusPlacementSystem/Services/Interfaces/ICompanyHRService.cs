using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.CompanyHR;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.Services.Interfaces;

public interface ICompanyHRService
{
    Task<ApiResponse<CompanyHRResponseDto>> RegisterCompanyHRAsync(RegisterCompanyHRRequestDto request);
    Task<ApiResponse<PagedResult<CompanyHRResponseDto>>> GetAllCompanyHRsAsync(PaginationParams paginationParams, AccountStatus? status = null, int? companyId = null);
    Task<ApiResponse<PagedResult<CompanyHRResponseDto>>> GetPendingCompanyHRsAsync(PaginationParams paginationParams);
    Task<ApiResponse<CompanyHRResponseDto>> GetCompanyHRByIdAsync(int id);
    Task<ApiResponse<CompanyHRResponseDto>> GetCurrentHRProfileAsync(string userId);
    Task<ApiResponse<CompanyHRResponseDto>> UpdateCurrentHRProfileAsync(string userId, UpdateCompanyHRProfileRequestDto request);
    Task<ApiResponse<bool>> ApproveCompanyHRAsync(int id, string approvedByUserId);
    Task<ApiResponse<bool>> RejectCompanyHRAsync(int id);
    Task<ApiResponse<bool>> SuspendCompanyHRAsync(int id);
}
