using System.ComponentModel.DataAnnotations;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.DTOs.Application;

public class UpdateApplicationStatusRequestDto
{
    [Required]
    public ApplicationStatus Status { get; set; }

    [MaxLength(1000)]
    public string? Remarks { get; set; }
}
