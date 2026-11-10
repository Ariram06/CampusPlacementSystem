using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Interview;

namespace CampusPlacementSystem.Services.Interfaces;

public interface IInterviewService
{
    // Interview Rounds
    Task<ApiResponse<InterviewRoundResponseDto>> CreateRoundAsync(CreateInterviewRoundRequestDto request, string hrUserId, string currentUserRole);
    Task<ApiResponse<List<InterviewRoundResponseDto>>> GetRoundsByJobIdAsync(int jobId, string currentUserId, string currentUserRole);

    // Interview Schedules
    Task<ApiResponse<InterviewScheduleResponseDto>> ScheduleInterviewAsync(ScheduleInterviewRequestDto request, string hrUserId, string currentUserRole);
    Task<ApiResponse<InterviewScheduleResponseDto>> UpdateScheduleAsync(int scheduleId, UpdateInterviewScheduleRequestDto request, string hrUserId, string currentUserRole);
    Task<ApiResponse<bool>> SubmitFeedbackAsync(int scheduleId, SubmitInterviewFeedbackRequestDto request, string hrUserId, string currentUserRole);
    Task<ApiResponse<InterviewScheduleResponseDto>> GetScheduleByIdAsync(int scheduleId, string currentUserId, string currentUserRole);
    Task<ApiResponse<PagedResult<InterviewScheduleResponseDto>>> GetSchedulesAsync(InterviewFilterParams filterParams, string currentUserId, string currentUserRole);
    Task<ApiResponse<bool>> CancelScheduleAsync(int scheduleId, string hrUserId, string currentUserRole);
}

