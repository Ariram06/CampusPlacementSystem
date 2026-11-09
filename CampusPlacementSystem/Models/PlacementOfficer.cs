using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.Models;

public class PlacementOfficer
{
    [Key]
    public int PlacementOfficerId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int CollegeId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Designation { get; set; } = string.Empty;

    public OfficerStatus Status { get; set; } = OfficerStatus.Pending;

    [MaxLength(450)]
    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

    [ForeignKey(nameof(CollegeId))]
    public College College { get; set; } = null!;

    public ICollection<Student> EnrolledStudents { get; set; } = new List<Student>();
}
