using System.Security.Claims;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.RecruitmentDrive;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecruitmentDriveController : ControllerBase
{
    private readonly IRecruitmentDriveService _driveService;

    public RecruitmentDriveController(IRecruitmentDriveService driveService)
    {
        _driveService = driveService;
    }

    [HttpPost]
    [Authorize(Roles = $"{AppRoles.CompanyHR},{AppRoles.SuperAdmin}")]
    public async Task<IActionResult> CreateDrive([FromBody] CreateRecruitmentDriveRequestDto request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _driveService.CreateDriveAsync(request, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetDriveById), new { id = result.Data!.DriveId }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{AppRoles.CompanyHR},{AppRoles.SuperAdmin}")]
    public async Task<IActionResult> UpdateDrive(int id, [FromBody] UpdateRecruitmentDriveRequestDto request)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _driveService.UpdateDriveAsync(id, request, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/cancel")]
    [Authorize(Roles = $"{AppRoles.CompanyHR},{AppRoles.SuperAdmin}")]
    public async Task<IActionResult> CancelDrive(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _driveService.CancelDriveAsync(id, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetDriveById(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _driveService.GetDriveByIdAsync(id, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetDrives([FromQuery] RecruitmentDriveFilterParams filterParams)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _driveService.GetDrivesAsync(filterParams, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}

