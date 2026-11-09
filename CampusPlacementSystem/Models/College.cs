using System.ComponentModel.DataAnnotations;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.Models;

public class College
{
    [Key]
    public int CollegeId { get; set; }

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

    public CollegeStatus Status { get; set; } = CollegeStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<PlacementOfficer> PlacementOfficers { get; set; } = new List<PlacementOfficer>();
    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<JobTargetCollege> TargetColleges { get; set; } = new List<JobTargetCollege>();
    public ICollection<RecruitmentDrive> RecruitmentDrives { get; set; } = new List<RecruitmentDrive>();
}
