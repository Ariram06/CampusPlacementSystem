using CampusPlacementSystem.Enums;

namespace CampusPlacementSystem.DTOs.Application;

public class ApplicationStatusHistoryResponseDto
{
    public int HistoryId { get; set; }
    public int ApplicationId { get; set; }
    public ApplicationStatus PreviousStatus { get; set; }
    public ApplicationStatus NewStatus { get; set; }
    public string UpdatedByUserId { get; set; } = string.Empty;
    public string UpdatedByUserName { get; set; } = string.Empty;
    public string? Remarks { get; set; }
    public DateTime UpdatedAt { get; set; }
}
