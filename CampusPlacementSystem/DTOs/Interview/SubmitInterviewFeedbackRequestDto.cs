using System.ComponentModel.DataAnnotations;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.DTOs.Interview;

public class SubmitInterviewFeedbackRequestDto
{
    [Required(ErrorMessage = "Feedback is required.")]
    [MaxLength(2000, ErrorMessage = "Feedback cannot exceed 2000 characters.")]
    public string Feedback { get; set; } = string.Empty;

    public InterviewStatus Status { get; set; } = InterviewStatus.Completed;
}
