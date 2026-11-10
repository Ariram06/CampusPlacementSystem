using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.DTOs.Interview;

public class InterviewFilterParams : PaginationParams
{
    public int? JobId { get; set; }
    public int? InterviewRoundId { get; set; }
    public int? ApplicationId { get; set; }
    public InterviewStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
