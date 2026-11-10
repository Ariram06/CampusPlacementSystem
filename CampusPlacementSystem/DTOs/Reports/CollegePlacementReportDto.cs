namespace CampusPlacementSystem.DTOs.Reports;

public class DepartmentPlacementSummaryDto
{
    public string Department { get; set; } = string.Empty;
    public int TotalStudents { get; set; }
    public int PlacedStudents { get; set; }
    public decimal PlacementRate { get; set; }
}

public class TopRecruiterDto
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int SelectedCount { get; set; }
    public decimal HighestPackage { get; set; }
}

public class CollegePlacementReportDto
{
    public int CollegeId { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public string CollegeCode { get; set; } = string.Empty;
    public int TotalEnrolledStudents { get; set; }
    public int ApprovedStudents { get; set; }
    public int PlacedStudents { get; set; }
    public decimal PlacementPercentage { get; set; }
    public int TotalApplicationsSubmitted { get; set; }
    public decimal HighestSalaryPackage { get; set; }
    public decimal AverageSalaryPackage { get; set; }
    public int TotalRecruitmentDrives { get; set; }
    public List<DepartmentPlacementSummaryDto> DepartmentBreakdown { get; set; } = new();
    public List<TopRecruiterDto> TopRecruiters { get; set; } = new();
}
