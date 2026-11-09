using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusPlacementSystem.Models;

public class JobTargetCollege
{
    [Key]
    public int JobTargetCollegeId { get; set; }

    public int JobId { get; set; }

    public int CollegeId { get; set; }

    // Navigation Properties
    [ForeignKey(nameof(JobId))]
    public JobOpening JobOpening { get; set; } = null!;

    [ForeignKey(nameof(CollegeId))]
    public College College { get; set; } = null!;
}
