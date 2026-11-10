using CampusPlacementSystem.Data;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Interview;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Models;
using CampusPlacementSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementSystem.Repositories.Implementations;

public class InterviewRepository : IInterviewRepository
{
    private readonly AppDbContext _context;

    public InterviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<InterviewRound?> GetRoundByIdAsync(int roundId)
    {
        return await _context.InterviewRounds
            .Include(r => r.JobOpening)
                .ThenInclude(j => j.Company)
            .Include(r => r.InterviewSchedules)
            .FirstOrDefaultAsync(r => r.InterviewRoundId == roundId);
    }

    public async Task<List<InterviewRound>> GetRoundsByJobIdAsync(int jobId)
    {
        return await _context.InterviewRounds
            .Include(r => r.JobOpening)
            .Include(r => r.InterviewSchedules)
            .Where(r => r.JobId == jobId)
            .OrderBy(r => r.RoundNumber)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> RoundNumberExistsForJobAsync(int jobId, int roundNumber, int? excludeRoundId = null)
    {
        var query = _context.InterviewRounds
            .Where(r => r.JobId == jobId && r.RoundNumber == roundNumber);

        if (excludeRoundId.HasValue)
        {
            query = query.Where(r => r.InterviewRoundId != excludeRoundId.Value);
        }

        return await query.AnyAsync();
    }

    public async Task<InterviewRound> AddRoundAsync(InterviewRound round)
    {
        await _context.InterviewRounds.AddAsync(round);
        await _context.SaveChangesAsync();
        return round;
    }

    public async Task<InterviewSchedule?> GetScheduleByIdAsync(int scheduleId)
    {
        return await _context.InterviewSchedules
            .Include(s => s.Application)
                .ThenInclude(a => a.Student)
                    .ThenInclude(st => st.User)
            .Include(s => s.Application)
                .ThenInclude(a => a.JobOpening)
                    .ThenInclude(j => j.Company)
            .Include(s => s.InterviewRound)
            .FirstOrDefaultAsync(s => s.InterviewScheduleId == scheduleId);
    }

    public async Task<InterviewSchedule?> GetScheduleByIdWithDetailsAsync(int scheduleId)
    {
        return await _context.InterviewSchedules
            .Include(s => s.Application)
                .ThenInclude(a => a.Student)
                    .ThenInclude(st => st.User)
            .Include(s => s.Application)
                .ThenInclude(a => a.Student)
                    .ThenInclude(st => st.College)
            .Include(s => s.Application)
                .ThenInclude(a => a.JobOpening)
                    .ThenInclude(j => j.Company)
            .Include(s => s.InterviewRound)
            .FirstOrDefaultAsync(s => s.InterviewScheduleId == scheduleId);
    }

    public async Task<bool> HasActiveScheduleForRoundAsync(int applicationId, int roundId)
    {
        return await _context.InterviewSchedules
            .AnyAsync(s => s.ApplicationId == applicationId
                        && s.InterviewRoundId == roundId
                        && (s.Status == InterviewStatus.Scheduled || s.Status == InterviewStatus.Rescheduled));
    }

    public async Task<PagedResult<InterviewScheduleResponseDto>> GetSchedulesAsync(
        InterviewFilterParams filterParams,
        int? companyId = null,
        int? studentId = null,
        int? collegeId = null)
    {
        var query = _context.InterviewSchedules
            .Include(s => s.Application)
                .ThenInclude(a => a.Student)
                    .ThenInclude(st => st.User)
            .Include(s => s.Application)
                .ThenInclude(a => a.Student)
                    .ThenInclude(st => st.College)
            .Include(s => s.Application)
                .ThenInclude(a => a.JobOpening)
                    .ThenInclude(j => j.Company)
            .Include(s => s.InterviewRound)
            .AsNoTracking()
            .AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(s => s.Application.JobOpening.CompanyId == companyId.Value);
        }

        if (studentId.HasValue)
        {
            query = query.Where(s => s.Application.StudentId == studentId.Value);
        }

        if (collegeId.HasValue)
        {
            query = query.Where(s => s.Application.Student.CollegeId == collegeId.Value);
        }

        if (filterParams.JobId.HasValue)
        {
            query = query.Where(s => s.Application.JobId == filterParams.JobId.Value);
        }

        if (filterParams.InterviewRoundId.HasValue)
        {
            query = query.Where(s => s.InterviewRoundId == filterParams.InterviewRoundId.Value);
        }

        if (filterParams.ApplicationId.HasValue)
        {
            query = query.Where(s => s.ApplicationId == filterParams.ApplicationId.Value);
        }

        if (filterParams.Status.HasValue)
        {
            query = query.Where(s => s.Status == filterParams.Status.Value);
        }

        if (filterParams.FromDate.HasValue)
        {
            query = query.Where(s => s.ScheduledAt >= filterParams.FromDate.Value);
        }

        if (filterParams.ToDate.HasValue)
        {
            query = query.Where(s => s.ScheduledAt <= filterParams.ToDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(filterParams.Search))
        {
            var search = filterParams.Search.Trim().ToLower();
            query = query.Where(s => s.Application.Student.User.FullName.ToLower().Contains(search)
                                  || s.Application.Student.RegisterNumber.ToLower().Contains(search)
                                  || s.Application.JobOpening.JobTitle.ToLower().Contains(search)
                                  || s.Application.JobOpening.Company.CompanyName.ToLower().Contains(search)
                                  || s.InterviewRound.RoundName.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(s => s.ScheduledAt)
            .Skip((filterParams.PageNumber - 1) * filterParams.PageSize)
            .Take(filterParams.PageSize)
            .Select(s => new InterviewScheduleResponseDto
            {
                InterviewScheduleId = s.InterviewScheduleId,
                ApplicationId = s.ApplicationId,
                StudentId = s.Application.StudentId,
                StudentName = s.Application.Student.User.FullName,
                StudentEmail = s.Application.Student.User.Email ?? string.Empty,
                RegisterNumber = s.Application.Student.RegisterNumber,
                CollegeId = s.Application.Student.CollegeId,
                CollegeName = s.Application.Student.College.CollegeName,
                JobId = s.Application.JobId,
                JobTitle = s.Application.JobOpening.JobTitle,
                CompanyId = s.Application.JobOpening.CompanyId,
                CompanyName = s.Application.JobOpening.Company.CompanyName,
                InterviewRoundId = s.InterviewRoundId,
                RoundName = s.InterviewRound.RoundName,
                RoundNumber = s.InterviewRound.RoundNumber,
                ScheduledAt = s.ScheduledAt,
                MeetingLink = s.MeetingLink,
                Venue = s.Venue,
                Status = s.Status,
                Feedback = s.Feedback,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return new PagedResult<InterviewScheduleResponseDto>(items, totalCount, filterParams.PageNumber, filterParams.PageSize);
    }

    public async Task<InterviewSchedule> AddScheduleAsync(InterviewSchedule schedule)
    {
        await _context.InterviewSchedules.AddAsync(schedule);
        await _context.SaveChangesAsync();
        return schedule;
    }

    public async Task UpdateScheduleAsync(InterviewSchedule schedule)
    {
        _context.InterviewSchedules.Update(schedule);
        await _context.SaveChangesAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

