using System.Security.Claims;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Interview;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InterviewController : ControllerBase
{
    private readonly IInterviewService _interviewService;

    public InterviewController(IInterviewService interviewService)
    {
        _interviewService = interviewService;
    }

    // --- Interview Rounds ---

    [HttpPost("round")]
    [Authorize(Roles = $"{AppRoles.CompanyHR},{AppRoles.SuperAdmin}")]
    public async Task<IActionResult> CreateRound([FromBody] CreateInterviewRoundRequestDto request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _interviewService.CreateRoundAsync(request, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("round/job/{jobId:int}")]
    [Authorize]
    public async Task<IActionResult> GetRoundsByJobId(int jobId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _interviewService.GetRoundsByJobIdAsync(jobId, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    // --- Interview Schedules ---

    [HttpPost("schedule")]
    [Authorize(Roles = $"{AppRoles.CompanyHR},{AppRoles.SuperAdmin}")]
    public async Task<IActionResult> ScheduleInterview([FromBody] ScheduleInterviewRequestDto request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _interviewService.ScheduleInterviewAsync(request, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetScheduleById), new { id = result.Data!.InterviewScheduleId }, result);
    }

    [HttpPut("schedule/{id:int}")]
    [Authorize(Roles = $"{AppRoles.CompanyHR},{AppRoles.SuperAdmin}")]
    public async Task<IActionResult> UpdateSchedule(int id, [FromBody] UpdateInterviewScheduleRequestDto request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _interviewService.UpdateScheduleAsync(id, request, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("schedule/{id:int}/feedback")]
    [Authorize(Roles = $"{AppRoles.CompanyHR},{AppRoles.SuperAdmin}")]
    public async Task<IActionResult> SubmitFeedback(int id, [FromBody] SubmitInterviewFeedbackRequestDto request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _interviewService.SubmitFeedbackAsync(id, request, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("schedule/{id:int}/cancel")]
    [Authorize(Roles = $"{AppRoles.CompanyHR},{AppRoles.SuperAdmin}")]
    public async Task<IActionResult> CancelSchedule(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _interviewService.CancelScheduleAsync(id, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("schedule/{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetScheduleById(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _interviewService.GetScheduleByIdAsync(id, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("schedule")]
    [Authorize]
    public async Task<IActionResult> GetSchedules([FromQuery] InterviewFilterParams filterParams)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _interviewService.GetSchedulesAsync(filterParams, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

