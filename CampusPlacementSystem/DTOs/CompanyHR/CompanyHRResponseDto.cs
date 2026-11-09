using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.DTOs.CompanyHR;

public class CompanyHRResponseDto
{
    public int CompanyHRId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public AccountStatus AccountStatus { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalJobsPosted { get; set; }
}
