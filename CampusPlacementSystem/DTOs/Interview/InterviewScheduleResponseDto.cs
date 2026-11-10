using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.DTOs.Interview;

public class InterviewScheduleResponseDto
{
    public int InterviewScheduleId { get; set; }
    public int ApplicationId { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string RegisterNumber { get; set; } = string.Empty;
    public int CollegeId { get; set; }
    public string CollegeName { get; set; } = string.Empty;
    public int JobId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public int InterviewRoundId { get; set; }
    public string RoundName { get; set; } = string.Empty;
    public int RoundNumber { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string? MeetingLink { get; set; }
    public string? Venue { get; set; }
    public InterviewStatus Status { get; set; }
    public string? Feedback { get; set; }
    public DateTime CreatedAt { get; set; }
}
