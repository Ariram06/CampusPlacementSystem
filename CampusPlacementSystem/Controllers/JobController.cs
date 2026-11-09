using System.Security.Claims;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Job;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.CompanyHR)]
    public async Task<IActionResult> CreateJob([FromBody] CreateJobRequestDto request)
    {
        var hrUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _jobService.CreateJobAsync(request, hrUserId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetJobById), new { id = result.Data!.JobId }, result);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllJobs([FromQuery] JobFilterParams filterParams)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _jobService.GetAllVisibleJobsAsync(filterParams, currentUserId, currentUserRole);
        return Ok(result);
    }

    [HttpGet("eligible")]
    [Authorize(Roles = AppRoles.Student)]
    public async Task<IActionResult> GetEligibleJobs([FromQuery] PaginationParams paginationParams)
    {
        var studentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _jobService.GetEligibleJobsForStudentAsync(studentUserId, paginationParams);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetJobById(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _jobService.GetJobByIdAsync(id, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet("{id:int}/check-eligibility")]
    [Authorize(Roles = AppRoles.Student)]
    public async Task<IActionResult> CheckEligibility(int id)
    {
        var studentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _jobService.CheckStudentEligibilityAsync(id, studentUserId);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{AppRoles.CompanyHR},{AppRoles.SuperAdmin}")]
    public async Task<IActionResult> UpdateJob(int id, [FromBody] UpdateJobRequestDto request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _jobService.UpdateJobAsync(id, request, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/publish")]
    [Authorize(Roles = $"{AppRoles.CompanyHR},{AppRoles.SuperAdmin}")]
    public async Task<IActionResult> PublishJob(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _jobService.PublishJobAsync(id, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/close")]
    [Authorize(Roles = $"{AppRoles.CompanyHR},{AppRoles.SuperAdmin}")]
    public async Task<IActionResult> CloseJob(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _jobService.CloseJobAsync(id, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/cancel")]
    [Authorize(Roles = $"{AppRoles.CompanyHR},{AppRoles.SuperAdmin}")]
    public async Task<IActionResult> CancelJob(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _jobService.CancelJobAsync(id, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = $"{AppRoles.CompanyHR},{AppRoles.SuperAdmin}")]
    public async Task<IActionResult> DeleteDraftJob(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _jobService.DeleteDraftJobAsync(id, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{id:int}/target-colleges")]
    [Authorize(Roles = $"{AppRoles.CompanyHR},{AppRoles.SuperAdmin}")]
    public async Task<IActionResult> AddTargetColleges(int id, [FromBody] AddTargetCollegesRequestDto request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _jobService.AddTargetCollegesAsync(id, request, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpDelete("{id:int}/target-colleges/{collegeId:int}")]
    [Authorize(Roles = $"{AppRoles.CompanyHR},{AppRoles.SuperAdmin}")]
    public async Task<IActionResult> RemoveTargetCollege(int id, int collegeId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _jobService.RemoveTargetCollegeAsync(id, collegeId, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
