using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.DTOs.Application;

public class ApplicationFilterParams : PaginationParams
{
    public ApplicationStatus? Status { get; set; }
    public int? JobId { get; set; }
    public int? CollegeId { get; set; }
    public string? Department { get; set; }
}
