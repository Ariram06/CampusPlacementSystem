using System.ComponentModel.DataAnnotations;

namespace CampusPlacementSystem.DTOs.Application;

public class ApplyJobRequestDto
{
    [Required]
    public int JobId { get; set; }

    [MaxLength(500)]
    public string? ResumeUrl { get; set; }
}
