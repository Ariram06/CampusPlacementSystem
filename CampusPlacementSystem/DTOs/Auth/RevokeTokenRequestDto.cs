using System.ComponentModel.DataAnnotations;

namespace CampusPlacementSystem.DTOs.Auth;

public class RevokeTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
