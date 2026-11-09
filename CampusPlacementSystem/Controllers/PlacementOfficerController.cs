using System.Security.Claims;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.PlacementOfficer;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlacementOfficerController : ControllerBase
{
    private readonly IPlacementOfficerService _officerService;

    public PlacementOfficerController(IPlacementOfficerService officerService)
    {
        _officerService = officerService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterOfficer([FromBody] RegisterOfficerRequestDto request)
    {
        var result = await _officerService.RegisterOfficerAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetOfficerById), new { id = result.Data!.PlacementOfficerId }, result);
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> GetAllOfficers(
        [FromQuery] PaginationParams paginationParams,
        [FromQuery] OfficerStatus? status = null,
        [FromQuery] int? collegeId = null)
    {
        var result = await _officerService.GetAllOfficersAsync(paginationParams, status, collegeId);
        return Ok(result);
    }

    [HttpGet("pending")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> GetPendingOfficers([FromQuery] PaginationParams paginationParams)
    {
        var result = await _officerService.GetPendingOfficersAsync(paginationParams);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize(Roles = AppRoles.PlacementOfficer)]
    public async Task<IActionResult> GetCurrentProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<OfficerResponseDto>.FailureResult("User not authenticated."));
        }

        var result = await _officerService.GetCurrentOfficerProfileAsync(userId);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPut("me")]
    [Authorize(Roles = AppRoles.PlacementOfficer)]
    public async Task<IActionResult> UpdateCurrentProfile([FromBody] UpdateOfficerProfileRequestDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<OfficerResponseDto>.FailureResult("User not authenticated."));
        }

        var result = await _officerService.UpdateOfficerProfileAsync(userId, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.PlacementOfficer}")]
    public async Task<IActionResult> GetOfficerById(int id)
    {
        var result = await _officerService.GetOfficerByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/approve")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> ApproveOfficer(int id)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _officerService.ApproveOfficerAsync(id, adminId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/reject")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> RejectOfficer(int id)
    {
        var result = await _officerService.RejectOfficerAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/suspend")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> SuspendOfficer(int id)
    {
        var result = await _officerService.SuspendOfficerAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
