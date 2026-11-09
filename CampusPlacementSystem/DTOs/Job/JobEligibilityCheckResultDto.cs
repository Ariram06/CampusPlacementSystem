namespace CampusPlacementSystem.DTOs.Job;

public class JobEligibilityCheckResultDto
{
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public bool IsEligible { get; set; }
    public string Summary { get; set; } = string.Empty;
    public List<string> FailedCriteria { get; set; } = new();
}
