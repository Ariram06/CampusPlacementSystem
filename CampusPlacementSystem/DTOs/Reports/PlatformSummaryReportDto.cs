namespace CampusPlacementSystem.DTOs.Reports;

public class PlatformSummaryReportDto
{
    public int TotalColleges { get; set; }
    public int ApprovedColleges { get; set; }
    public int PendingColleges { get; set; }
    public int TotalCompanies { get; set; }
    public int ApprovedCompanies { get; set; }
    public int TotalStudents { get; set; }
    public int ApprovedStudents { get; set; }
    public int PlacedStudents { get; set; }
    public decimal OverallPlacementRate { get; set; }
    public int TotalJobOpenings { get; set; }
    public int PublishedJobOpenings { get; set; }
    public int TotalApplications { get; set; }
    public int TotalRecruitmentDrives { get; set; }
    public int TotalInterviewsConducted { get; set; }
}
