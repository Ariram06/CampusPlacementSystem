using System.ComponentModel.DataAnnotations;

namespace CampusPlacementSystem.DTOs.College;

public class CreateCollegeRequestDto
{
    [Required]
    [MaxLength(200)]
    public string CollegeName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string CollegeCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string UniversityName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string OfficialEmail { get; set; } = string.Empty;

    [Required]
    [Phone]
    [MaxLength(20)]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Website { get; set; }
}
