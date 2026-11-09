using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusPlacementSystem.Models;

public class InterviewRound
{
    [Key]
    public int InterviewRoundId { get; set; }

    public int JobId { get; set; }

    [Required]
    [MaxLength(100)]
    public string RoundName { get; set; } = string.Empty;

    public int RoundNumber { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(JobId))]
    public JobOpening JobOpening { get; set; } = null!;

    public ICollection<InterviewSchedule> InterviewSchedules { get; set; } = new List<InterviewSchedule>();
}
