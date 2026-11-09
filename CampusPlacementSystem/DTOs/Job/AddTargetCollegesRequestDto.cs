using System.ComponentModel.DataAnnotations;

namespace CampusPlacementSystem.DTOs.Job;

public class AddTargetCollegesRequestDto
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one college must be selected.")]
    public List<int> CollegeIds { get; set; } = new();
}
