using System.ComponentModel.DataAnnotations;

namespace CampusPlacementSystem.DTOs.Student;

public class UpdateStudentProfileRequestDto
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [Required]
    [Range(0.00, 10.00, ErrorMessage = "CGPA must be between 0.00 and 10.00.")]
    public decimal CGPA { get; set; }

    [Range(0, 100, ErrorMessage = "Active backlogs cannot be negative.")]
    public int ActiveBacklogs { get; set; }

    [MaxLength(500)]
    public string? ResumeUrl { get; set; }
}
