namespace CampusPlacementSystem.DTOs.Auth;

public class LoginResponseDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int? AssociatedCollegeId { get; set; }
    public int? AssociatedCompanyId { get; set; }
    public int? ProfileId { get; set; }
}
