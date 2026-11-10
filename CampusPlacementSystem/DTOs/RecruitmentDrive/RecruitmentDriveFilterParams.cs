using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.DTOs.RecruitmentDrive;

public class RecruitmentDriveFilterParams : PaginationParams
{
    public int? JobId { get; set; }
    public int? CollegeId { get; set; }
    public DriveStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
