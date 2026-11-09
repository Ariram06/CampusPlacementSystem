using System.ComponentModel.DataAnnotations;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.Models;

public class Company
{
    [Key]
    public int CompanyId { get; set; }

    [Required]
    [MaxLength(200)]
    public string CompanyName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string CompanyEmail { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Website { get; set; }

    [Required]
    [MaxLength(100)]
    public string Industry { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public CompanyStatus Status { get; set; } = CompanyStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<CompanyHR> HRUsers { get; set; } = new List<CompanyHR>();
    public ICollection<JobOpening> JobOpenings { get; set; } = new List<JobOpening>();
}
