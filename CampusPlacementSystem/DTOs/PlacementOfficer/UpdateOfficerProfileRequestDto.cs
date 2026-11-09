using System.ComponentModel.DataAnnotations;

namespace CampusPlacementSystem.DTOs.PlacementOfficer;

public class UpdateOfficerProfileRequestDto
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Designation { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }
}
