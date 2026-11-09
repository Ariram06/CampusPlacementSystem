using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.Models;

public class JobOpening
{
    [Key]
    public int JobId { get; set; }

    public int CompanyId { get; set; }

    public int CreatedByHRId { get; set; }

    [Required]
    [MaxLength(200)]
    public string JobTitle { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string JobDescription { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string JobType { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string Location { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal SalaryPackage { get; set; }

    [Column(TypeName = "decimal(3, 2)")]
    public decimal MinimumCGPA { get; set; }

    public int MaximumBacklogs { get; set; } = 0;

    public DateTime ApplicationDeadline { get; set; }

    public bool IsOpenToAllColleges { get; set; } = false;

    public JobStatus Status { get; set; } = JobStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey(nameof(CompanyId))]
    public Company Company { get; set; } = null!;

    [ForeignKey(nameof(CreatedByHRId))]
    public CompanyHR CreatedByHR { get; set; } = null!;

    public ICollection<JobEligibleDepartment> EligibleDepartments { get; set; } = new List<JobEligibleDepartment>();
    public ICollection<JobEligibleGraduationYear> EligibleGraduationYears { get; set; } = new List<JobEligibleGraduationYear>();
    public ICollection<JobTargetCollege> TargetColleges { get; set; } = new List<JobTargetCollege>();
    public ICollection<JobApplication> Applications { get; set; } = new List<JobApplication>();
    public ICollection<RecruitmentDrive> RecruitmentDrives { get; set; } = new List<RecruitmentDrive>();
    public ICollection<InterviewRound> InterviewRounds { get; set; } = new List<InterviewRound>();
}
