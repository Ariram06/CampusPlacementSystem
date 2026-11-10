using CampusPlacementSystem.DTOs.Reports;

namespace CampusPlacementSystem.Repositories.Interfaces;

public interface IReportRepository
{
    Task<PlatformSummaryReportDto> GetPlatformSummaryAsync();
    Task<CollegePlacementReportDto?> GetCollegePlacementReportAsync(int collegeId);
    Task<CompanyRecruitmentReportDto?> GetCompanyRecruitmentReportAsync(int companyId);
    Task<StudentPlacementStatusDto?> GetStudentPlacementReportAsync(int studentId);
}
