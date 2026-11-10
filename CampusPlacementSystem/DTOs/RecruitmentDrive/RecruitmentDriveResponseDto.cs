using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.DTOs.RecruitmentDrive;

public class RecruitmentDriveResponseDto
{
    public int DriveId { get; set; }
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int CollegeId { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public string CollegeCode { get; set; } = string.Empty;
    public DateTime DriveDate { get; set; }
    public string Venue { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DriveStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}
