using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.Models;

public class ApplicationStatusHistory
{
    [Key]
    public int HistoryId { get; set; }

    public int ApplicationId { get; set; }

    public ApplicationStatus PreviousStatus { get; set; }

    public ApplicationStatus NewStatus { get; set; }

    [Required]
    [MaxLength(450)]
    public string UpdatedByUserId { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Remarks { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey(nameof(ApplicationId))]
    public JobApplication Application { get; set; } = null!;

    [ForeignKey(nameof(UpdatedByUserId))]
    public ApplicationUser UpdatedByUser { get; set; } = null!;
}
