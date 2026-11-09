using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Company;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.Services.Interfaces;

public interface ICompanyService
{
    Task<ApiResponse<CompanyResponseDto>> RegisterCompanyAsync(CreateCompanyRequestDto request);
    Task<ApiResponse<PagedResult<CompanyResponseDto>>> GetAllCompaniesAsync(PaginationParams paginationParams, CompanyStatus? status = null);
    Task<ApiResponse<PagedResult<CompanyResponseDto>>> GetPendingCompaniesAsync(PaginationParams paginationParams);
    Task<ApiResponse<CompanyResponseDto>> GetCompanyByIdAsync(int id);
    Task<ApiResponse<CompanyResponseDto>> UpdateCompanyAsync(int id, UpdateCompanyRequestDto request, string currentUserId, string currentUserRole);
    Task<ApiResponse<bool>> ApproveCompanyAsync(int id);
    Task<ApiResponse<bool>> RejectCompanyAsync(int id);
    Task<ApiResponse<bool>> SuspendCompanyAsync(int id);
}
