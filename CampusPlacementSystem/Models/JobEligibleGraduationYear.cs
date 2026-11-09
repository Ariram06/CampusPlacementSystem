using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusPlacementSystem.Models;

public class JobEligibleGraduationYear
{
    [Key]
    public int Id { get; set; }

    public int JobId { get; set; }

    public int GraduationYear { get; set; }

    [ForeignKey(nameof(JobId))]
    public JobOpening JobOpening { get; set; } = null!;
}
