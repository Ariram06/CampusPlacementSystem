using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.Models;

public class InterviewSchedule
{
    [Key]
    public int InterviewScheduleId { get; set; }

    public int ApplicationId { get; set; }

    public int InterviewRoundId { get; set; }

    public DateTime ScheduledAt { get; set; }

    [MaxLength(500)]
    public string? MeetingLink { get; set; }

    [MaxLength(250)]
    public string? Venue { get; set; }

    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

    [MaxLength(2000)]
    public string? Feedback { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey(nameof(ApplicationId))]
    public JobApplication Application { get; set; } = null!;

    [ForeignKey(nameof(InterviewRoundId))]
    public InterviewRound InterviewRound { get; set; } = null!;
}
