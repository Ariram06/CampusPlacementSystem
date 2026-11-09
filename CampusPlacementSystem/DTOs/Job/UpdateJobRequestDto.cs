using System.ComponentModel.DataAnnotations;

namespace CampusPlacementSystem.DTOs.Job;

public class UpdateJobRequestDto
{
    [Required]
    [MaxLength(200)]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string JobDescription { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string JobType { get; set; } = "Full-Time";

    [Required]
    [MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    [Required]
    [Range(0, 100000000, ErrorMessage = "Salary package must be a positive value.")]
    public decimal SalaryPackage { get; set; }

    [Required]
    [Range(0.00, 10.00, ErrorMessage = "Minimum CGPA must be between 0.00 and 10.00.")]
    public decimal MinimumCGPA { get; set; }

    [Range(0, 100, ErrorMessage = "Maximum backlogs cannot be negative.")]
    public int MaximumBacklogs { get; set; }

    [Required]
    public DateTime ApplicationDeadline { get; set; }

    public bool IsOpenToAllColleges { get; set; }

    public List<string> EligibleDepartments { get; set; } = new();

    public List<int> EligibleGraduationYears { get; set; } = new();
}
