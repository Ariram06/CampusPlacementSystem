using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CampusPlacementSystem.Models;

public class CompanyHR
{
    [Key]
    public int CompanyHRId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public int CompanyId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Designation { get; set; } = string.Empty;

    [MaxLength(450)]
    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;

    [ForeignKey(nameof(CompanyId))]
    public Company Company { get; set; } = null!;

    public ICollection<JobOpening> CreatedJobOpenings { get; set; } = new List<JobOpening>();
}
