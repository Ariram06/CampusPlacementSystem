using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Reports;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Repositories.Interfaces;
using CampusPlacementSystem.Services.Interfaces;

namespace CampusPlacementSystem.Services.Implementations;

public class ReportService : IReportService
{
    private readonly IReportRepository _reportRepository;
    private readonly IPlacementOfficerRepository _officerRepository;
    private readonly ICompanyHRRepository _hrRepository;
    private readonly IStudentRepository _studentRepository;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IReportRepository reportRepository,
        IPlacementOfficerRepository officerRepository,
        ICompanyHRRepository hrRepository,
        IStudentRepository studentRepository,
        ILogger<ReportService> logger)
    {
        _reportRepository = reportRepository;
        _officerRepository = officerRepository;
        _hrRepository = hrRepository;
        _studentRepository = studentRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<PlatformSummaryReportDto>> GetPlatformSummaryAsync()
    {
        var summary = await _reportRepository.GetPlatformSummaryAsync();
        return ApiResponse<PlatformSummaryReportDto>.SuccessResult(summary);
    }

    public async Task<ApiResponse<CollegePlacementReportDto>> GetCollegePlacementReportAsync(
        string currentUserId,
        string currentUserRole,
        int? requestedCollegeId = null)
    {
        int targetCollegeId;

        if (currentUserRole == AppRoles.SuperAdmin)
        {
            if (!requestedCollegeId.HasValue)
            {
                return ApiResponse<CollegePlacementReportDto>.FailureResult("Please specify a collegeId to view the institutional placement report.");
            }
            targetCollegeId = requestedCollegeId.Value;
        }
        else if (currentUserRole == AppRoles.PlacementOfficer)
        {
            var officer = await _officerRepository.GetByUserIdAsync(currentUserId);
            if (officer == null)
            {
                return ApiResponse<CollegePlacementReportDto>.FailureResult("Placement officer profile not found.");
            }

            if (requestedCollegeId.HasValue && requestedCollegeId.Value != officer.CollegeId)
            {
                return ApiResponse<CollegePlacementReportDto>.FailureResult("You can only view placement reports for your own college.");
            }

            targetCollegeId = officer.CollegeId;
        }
        else
        {
            return ApiResponse<CollegePlacementReportDto>.FailureResult("You are not authorized to view college placement reports.");
        }

        var report = await _reportRepository.GetCollegePlacementReportAsync(targetCollegeId);
        if (report == null)
        {
            return ApiResponse<CollegePlacementReportDto>.FailureResult("College not found.");
        }

        return ApiResponse<CollegePlacementReportDto>.SuccessResult(report);
    }

    public async Task<ApiResponse<CompanyRecruitmentReportDto>> GetCompanyRecruitmentReportAsync(
        string currentUserId,
        string currentUserRole,
        int? requestedCompanyId = null)
    {
        int targetCompanyId;

        if (currentUserRole == AppRoles.SuperAdmin)
        {
            if (!requestedCompanyId.HasValue)
            {
                return ApiResponse<CompanyRecruitmentReportDto>.FailureResult("Please specify a companyId to view the corporate recruitment report.");
            }
            targetCompanyId = requestedCompanyId.Value;
        }
        else if (currentUserRole == AppRoles.CompanyHR)
        {
            var hr = await _hrRepository.GetByUserIdAsync(currentUserId);
            if (hr == null)
            {
                return ApiResponse<CompanyRecruitmentReportDto>.FailureResult("Company HR profile not found.");
            }

            if (requestedCompanyId.HasValue && requestedCompanyId.Value != hr.CompanyId)
            {
                return ApiResponse<CompanyRecruitmentReportDto>.FailureResult("You can only view recruitment reports for your own company.");
            }

            targetCompanyId = hr.CompanyId;
        }
        else
        {
            return ApiResponse<CompanyRecruitmentReportDto>.FailureResult("You are not authorized to view corporate recruitment reports.");
        }

        var report = await _reportRepository.GetCompanyRecruitmentReportAsync(targetCompanyId);
        if (report == null)
        {
            return ApiResponse<CompanyRecruitmentReportDto>.FailureResult("Company not found.");
        }

        return ApiResponse<CompanyRecruitmentReportDto>.SuccessResult(report);
    }

    public async Task<ApiResponse<StudentPlacementStatusDto>> GetStudentPlacementReportAsync(string studentUserId)
    {
        var student = await _studentRepository.GetByUserIdAsync(studentUserId);
        if (student == null)
        {
            return ApiResponse<StudentPlacementStatusDto>.FailureResult("Student profile not found.");
        }

        var report = await _reportRepository.GetStudentPlacementReportAsync(student.StudentId);
        if (report == null)
        {
            return ApiResponse<StudentPlacementStatusDto>.FailureResult("Student placement records not found.");
        }

        return ApiResponse<StudentPlacementStatusDto>.SuccessResult(report);
    }
}
