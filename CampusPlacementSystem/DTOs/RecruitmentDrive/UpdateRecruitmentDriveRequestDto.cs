using System.ComponentModel.DataAnnotations;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.DTOs.RecruitmentDrive;

public class UpdateRecruitmentDriveRequestDto
{
    [Required(ErrorMessage = "Drive date is required.")]
    public DateTime DriveDate { get; set; }

    [Required(ErrorMessage = "Venue is required.")]
    [MaxLength(250, ErrorMessage = "Venue cannot exceed 250 characters.")]
    public string Venue { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public DriveStatus Status { get; set; }
}

