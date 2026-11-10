using System.ComponentModel.DataAnnotations;

namespace CampusPlacementSystem.DTOs.RecruitmentDrive;

public class CreateRecruitmentDriveRequestDto
{
    [Required(ErrorMessage = "Job ID is required.")]
    public int JobId { get; set; }

    [Required(ErrorMessage = "College ID is required.")]
    public int CollegeId { get; set; }

    [Required(ErrorMessage = "Drive date is required.")]
    public DateTime DriveDate { get; set; }

    [Required(ErrorMessage = "Venue is required.")]
    [MaxLength(250, ErrorMessage = "Venue cannot exceed 250 characters.")]
    public string Venue { get; set; } = string.Empty;

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string? Description { get; set; }
}
