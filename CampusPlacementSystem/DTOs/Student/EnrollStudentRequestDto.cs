using System.ComponentModel.DataAnnotations;

namespace CampusPlacementSystem.DTOs.Student;

public class EnrollStudentRequestDto
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    public string Password { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string RegisterNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Degree { get; set; } = string.Empty;

    [Required]
    [Range(2000, 2100, ErrorMessage = "Graduation year must be a valid year.")]
    public int GraduationYear { get; set; }

    [Required]
    [Range(0.00, 10.00, ErrorMessage = "CGPA must be between 0.00 and 10.00.")]
    public decimal CGPA { get; set; }

    [Range(0, 100, ErrorMessage = "Active backlogs cannot be negative.")]
    public int ActiveBacklogs { get; set; } = 0;

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [MaxLength(500)]
    public string? ResumeUrl { get; set; }
}
