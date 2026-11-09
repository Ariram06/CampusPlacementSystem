using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusPlacementSystem.Models;

public class JobEligibleDepartment
{
    [Key]
    public int Id { get; set; }

    public int JobId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [ForeignKey(nameof(JobId))]
    public JobOpening JobOpening { get; set; } = null!;
}
