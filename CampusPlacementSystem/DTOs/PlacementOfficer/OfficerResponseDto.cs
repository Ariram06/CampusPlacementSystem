using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.DTOs.PlacementOfficer;

public class OfficerResponseDto
{
    public int PlacementOfficerId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int CollegeId { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public string CollegeCode { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public OfficerStatus Status { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public int TotalEnrolledStudents { get; set; }
}
