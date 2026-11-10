namespace CampusPlacementSystem.DTOs.Reports;

public class StudentOfferDto
{
    public int ApplicationId { get; set; }
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public decimal SalaryPackage { get; set; }
    public DateTime SelectedAt { get; set; }
}

public class StudentPlacementStatusDto
{
    public int StudentId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string RegisterNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public int TotalApplicationsSubmitted { get; set; }
    public int UnderReviewApplications { get; set; }
    public int ShortlistedApplications { get; set; }
    public int InterviewsScheduledCount { get; set; }
    public int SelectedOffersCount { get; set; }
    public bool IsPlaced => SelectedOffersCount > 0;
    public List<StudentOfferDto> OffersReceived { get; set; } = new();
}
