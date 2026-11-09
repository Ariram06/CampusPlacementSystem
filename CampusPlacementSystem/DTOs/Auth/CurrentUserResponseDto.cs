using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.DTOs.Auth;

public class CurrentUserResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public AccountStatus AccountStatus { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? ProfileId { get; set; }
    public int? AssociatedCollegeId { get; set; }
    public string? AssociatedCollegeName { get; set; }
    public int? AssociatedCompanyId { get; set; }
    public string? AssociatedCompanyName { get; set; }
}
