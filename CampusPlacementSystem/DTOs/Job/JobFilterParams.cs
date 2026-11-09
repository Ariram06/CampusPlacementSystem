using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.DTOs.Job;

public class JobFilterParams : PaginationParams
{
    public JobStatus? Status { get; set; }
    public string? JobType { get; set; }
    public string? Location { get; set; }
    public decimal? MinSalary { get; set; }
    public int? CompanyId { get; set; }
}
