using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.DTOs.Company;

public class CompanyResponseDto
{
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyEmail { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string Industry { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    public CompanyStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalHRUsers { get; set; }
    public int TotalJobOpenings { get; set; }
}
