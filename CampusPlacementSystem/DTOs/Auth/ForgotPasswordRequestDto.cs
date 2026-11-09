using System.ComponentModel.DataAnnotations;

namespace CampusPlacementSystem.DTOs.Auth;

public class ForgotPasswordRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
