using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.Models;

public class RecruitmentDrive
{
    [Key]
    public int DriveId { get; set; }

    public int JobId { get; set; }

    public int CollegeId { get; set; }

    public DateTime DriveDate { get; set; }

    [Required]
    [MaxLength(250)]
    public string Venue { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public DriveStatus Status { get; set; } = DriveStatus.Scheduled;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey(nameof(JobId))]
    public JobOpening JobOpening { get; set; } = null!;

    [ForeignKey(nameof(CollegeId))]
    public College College { get; set; } = null!;
}
