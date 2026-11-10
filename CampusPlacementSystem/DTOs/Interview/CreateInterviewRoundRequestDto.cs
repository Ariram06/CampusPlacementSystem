using System.ComponentModel.DataAnnotations;

namespace CampusPlacementSystem.DTOs.Interview;

public class CreateInterviewRoundRequestDto
{
    [Required(ErrorMessage = "Job ID is required.")]
    public int JobId { get; set; }

    [Required(ErrorMessage = "Round name is required.")]
    [MaxLength(100, ErrorMessage = "Round name cannot exceed 100 characters.")]
    public string RoundName { get; set; } = string.Empty;

    [Range(1, 100, ErrorMessage = "Round number must be between 1 and 100.")]
    public int RoundNumber { get; set; }

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string? Description { get; set; }
}

