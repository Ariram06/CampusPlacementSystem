using System.Security.Claims;
using CampusPlacementSystem.DTOs.College;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CollegeController : ControllerBase
{
    private readonly ICollegeService _collegeService;

    public CollegeController(ICollegeService collegeService)
    {
        _collegeService = collegeService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterCollege([FromBody] CreateCollegeRequestDto request)
    {
        var result = await _collegeService.RegisterCollegeAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetCollegeById), new { id = result.Data!.CollegeId }, result);
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> GetAllColleges([FromQuery] PaginationParams paginationParams, [FromQuery] CollegeStatus? status = null)
    {
        var result = await _collegeService.GetAllCollegesAsync(paginationParams, status);
        return Ok(result);
    }

    [HttpGet("pending")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> GetPendingColleges([FromQuery] PaginationParams paginationParams)
    {
        var result = await _collegeService.GetPendingCollegesAsync(paginationParams);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.PlacementOfficer}")]
    public async Task<IActionResult> GetCollegeById(int id)
    {
        var result = await _collegeService.GetCollegeByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.PlacementOfficer}")]
    public async Task<IActionResult> UpdateCollege(int id, [FromBody] UpdateCollegeRequestDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _collegeService.UpdateCollegeAsync(id, request, userId, role);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/approve")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> ApproveCollege(int id)
    {
        var result = await _collegeService.ApproveCollegeAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/reject")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> RejectCollege(int id)
    {
        var result = await _collegeService.RejectCollegeAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/suspend")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> SuspendCollege(int id)
    {
        var result = await _collegeService.SuspendCollegeAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
