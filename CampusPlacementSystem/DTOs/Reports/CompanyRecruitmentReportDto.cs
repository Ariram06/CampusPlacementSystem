namespace CampusPlacementSystem.DTOs.Reports;

public class JobHiringSummaryDto
{
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal SalaryPackage { get; set; }
    public int TotalApplications { get; set; }
    public int Shortlisted { get; set; }
    public int Selected { get; set; }
}

public class CompanyRecruitmentReportDto
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int TotalJobsPosted { get; set; }
    public int PublishedJobs { get; set; }
    public int TotalApplicationsReceived { get; set; }
    public int UnderReviewApplications { get; set; }
    public int ShortlistedCandidates { get; set; }
    public int SelectedCandidates { get; set; }
    public int TotalInterviewsScheduled { get; set; }
    public int TotalInterviewsCompleted { get; set; }
    public int TotalRecruitmentDrives { get; set; }
    public List<JobHiringSummaryDto> JobBreakdown { get; set; } = new();
}
