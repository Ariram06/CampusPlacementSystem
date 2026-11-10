using System.ComponentModel.DataAnnotations;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.DTOs.Interview;

public class UpdateInterviewScheduleRequestDto
{
    [Required(ErrorMessage = "Scheduled date and time is required.")]
    public DateTime ScheduledAt { get; set; }

    [MaxLength(500, ErrorMessage = "Meeting link cannot exceed 500 characters.")]
    public string? MeetingLink { get; set; }

    [MaxLength(250, ErrorMessage = "Venue cannot exceed 250 characters.")]
    public string? Venue { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public InterviewStatus Status { get; set; }
}
