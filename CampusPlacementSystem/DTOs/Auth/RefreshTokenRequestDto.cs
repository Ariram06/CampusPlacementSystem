using System.ComponentModel.DataAnnotations;

namespace CampusPlacementSystem.DTOs.Auth;

public class RefreshTokenRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
