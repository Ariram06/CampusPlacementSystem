using System.Security.Claims;
using CampusPlacementSystem.DTOs.Application;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApplicationController : ControllerBase
{
    private readonly IApplicationService _applicationService;

    public ApplicationController(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    [HttpPost("apply")]
    [Authorize(Roles = AppRoles.Student)]
    public async Task<IActionResult> ApplyJob([FromBody] ApplyJobRequestDto request)
    {
        var studentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _applicationService.ApplyJobAsync(request, studentUserId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetApplicationById), new { id = result.Data!.ApplicationId }, result);
    }

    [HttpGet("my")]
    [Authorize(Roles = AppRoles.Student)]
    public async Task<IActionResult> GetMyApplications([FromQuery] ApplicationFilterParams filterParams)
    {
        var studentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _applicationService.GetMyApplicationsAsync(studentUserId, filterParams);
        return Ok(result);
    }

    [HttpGet("job/{jobId:int}")]
    [Authorize(Roles = $"{AppRoles.CompanyHR},{AppRoles.SuperAdmin}")]
    public async Task<IActionResult> GetJobApplicants(int jobId, [FromQuery] ApplicationFilterParams filterParams)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _applicationService.GetJobApplicantsAsync(jobId, filterParams, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetApplicationById(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _applicationService.GetApplicationByIdAsync(id, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = $"{AppRoles.CompanyHR},{AppRoles.SuperAdmin}")]
    public async Task<IActionResult> UpdateApplicationStatus(int id, [FromBody] UpdateApplicationStatusRequestDto request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _applicationService.UpdateApplicationStatusAsync(id, request, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/withdraw")]
    [Authorize(Roles = AppRoles.Student)]
    public async Task<IActionResult> WithdrawApplication(int id)
    {
        var studentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _applicationService.WithdrawApplicationAsync(id, studentUserId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id:int}/history")]
    [Authorize]
    public async Task<IActionResult> GetApplicationStatusHistory(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _applicationService.GetApplicationStatusHistoryAsync(id, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
