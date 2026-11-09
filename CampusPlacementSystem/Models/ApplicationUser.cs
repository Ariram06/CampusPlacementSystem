using System.ComponentModel.DataAnnotations;
using CampusPlacementSystem.Enums;
using Microsoft.AspNetCore.Identity;

namespace CampusPlacementSystem.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    public AccountStatus AccountStatus { get; set; } = AccountStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
