using CampusPlacementSystem.DTOs.College;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.DTOs.Job;

public class JobResponseDto
{
    public int JobId { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int CreatedByHRId { get; set; }
    public string HRName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal SalaryPackage { get; set; }
    public decimal MinimumCGPA { get; set; }
    public int MaximumBacklogs { get; set; }
    public DateTime ApplicationDeadline { get; set; }
    public bool IsOpenToAllColleges { get; set; }
    public JobStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> EligibleDepartments { get; set; } = new();
    public List<int> EligibleGraduationYears { get; set; } = new();
    public List<CollegeResponseDto> TargetColleges { get; set; } = new();
    public int TotalApplications { get; set; }
    public bool IsDeadlinePassed => DateTime.UtcNow > ApplicationDeadline;
}
