using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Reports;

namespace CampusPlacementSystem.Services.Interfaces;

public interface IReportService
{
    Task<ApiResponse<PlatformSummaryReportDto>> GetPlatformSummaryAsync();
    Task<ApiResponse<CollegePlacementReportDto>> GetCollegePlacementReportAsync(string currentUserId, string currentUserRole, int? requestedCollegeId = null);
    Task<ApiResponse<CompanyRecruitmentReportDto>> GetCompanyRecruitmentReportAsync(string currentUserId, string currentUserRole, int? requestedCompanyId = null);
    Task<ApiResponse<StudentPlacementStatusDto>> GetStudentPlacementReportAsync(string studentUserId);
}
