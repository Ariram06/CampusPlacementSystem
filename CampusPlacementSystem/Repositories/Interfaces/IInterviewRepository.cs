using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Interview;
using CampusPlacementSystem.Models;

namespace CampusPlacementSystem.Repositories.Interfaces;

public interface IInterviewRepository
{
    // Interview Rounds
    Task<InterviewRound?> GetRoundByIdAsync(int roundId);
    Task<List<InterviewRound>> GetRoundsByJobIdAsync(int jobId);
    Task<bool> RoundNumberExistsForJobAsync(int jobId, int roundNumber, int? excludeRoundId = null);
    Task<InterviewRound> AddRoundAsync(InterviewRound round);

    // Interview Schedules
    Task<InterviewSchedule?> GetScheduleByIdAsync(int scheduleId);
    Task<InterviewSchedule?> GetScheduleByIdWithDetailsAsync(int scheduleId);
    Task<bool> HasActiveScheduleForRoundAsync(int applicationId, int roundId);
    Task<PagedResult<InterviewScheduleResponseDto>> GetSchedulesAsync(
        InterviewFilterParams filterParams,
        int? companyId = null,
        int? studentId = null,
        int? collegeId = null);
    Task<InterviewSchedule> AddScheduleAsync(InterviewSchedule schedule);
    Task UpdateScheduleAsync(InterviewSchedule schedule);
    Task SaveChangesAsync();
}

