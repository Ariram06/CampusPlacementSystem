using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.Models;

public class JobApplication
{
    [Key]
    public int ApplicationId { get; set; }

    public int JobId { get; set; }

    public int StudentId { get; set; }

    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

    [MaxLength(500)]
    public string? ResumeUrl { get; set; }

    public DateTime? LastUpdatedAt { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(JobId))]
    public JobOpening JobOpening { get; set; } = null!;

    [ForeignKey(nameof(StudentId))]
    public Student Student { get; set; } = null!;

    public ICollection<InterviewSchedule> InterviewSchedules { get; set; } = new List<InterviewSchedule>();
    public ICollection<ApplicationStatusHistory> StatusHistories { get; set; } = new List<ApplicationStatusHistory>();
}
