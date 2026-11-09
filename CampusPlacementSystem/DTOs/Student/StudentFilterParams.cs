using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.DTOs.Student;

public class StudentFilterParams : PaginationParams
{
    public string? Department { get; set; }
    public string? Degree { get; set; }
    public int? GraduationYear { get; set; }
    public decimal? MinCGPA { get; set; }
    public int? MaxBacklogs { get; set; }
    public StudentStatus? Status { get; set; }
    public int? CollegeId { get; set; }
}
