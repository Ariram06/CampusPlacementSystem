using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.Models;

public class Student
{
    [Key]
    public int StudentId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int CollegeId { get; set; }

    public int PlacementOfficerId { get; set; }

    [Required]
    [MaxLength(50)]
    public string RegisterNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Degree { get; set; } = string.Empty;

    public int GraduationYear { get; set; }

    [Column(TypeName = "decimal(3, 2)")]
    public decimal CGPA { get; set; }

    public int ActiveBacklogs { get; set; } = 0;

    [MaxLength(500)]
    public string? ResumeUrl { get; set; }

    public StudentStatus Status { get; set; } = StudentStatus.Pending;

    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

    [ForeignKey(nameof(CollegeId))]
    public College College { get; set; } = null!;

    [ForeignKey(nameof(PlacementOfficerId))]
    public PlacementOfficer EnrolledByOfficer { get; set; } = null!;

    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
}
