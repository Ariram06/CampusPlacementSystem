using System.ComponentModel.DataAnnotations;

namespace CampusPlacementSystem.DTOs.Company;

public class CreateCompanyRequestDto
{
    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string CompanyEmail { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Website { get; set; }

    [Required]
    [MaxLength(100)]
    public string Industry { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }
}
