using System.Security.Claims;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.CompanyHR;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyHRController : ControllerBase
{
    private readonly ICompanyHRService _companyHRService;

    public CompanyHRController(ICompanyHRService companyHRService)
    {
        _companyHRService = companyHRService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterCompanyHR([FromBody] RegisterCompanyHRRequestDto request)
    {
        var result = await _companyHRService.RegisterCompanyHRAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetCompanyHRById), new { id = result.Data!.CompanyHRId }, result);
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> GetAllCompanyHRs(
        [FromQuery] PaginationParams paginationParams,
        [FromQuery] AccountStatus? status = null,
        [FromQuery] int? companyId = null)
    {
        var result = await _companyHRService.GetAllCompanyHRsAsync(paginationParams, status, companyId);
        return Ok(result);
    }

    [HttpGet("pending")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> GetPendingCompanyHRs([FromQuery] PaginationParams paginationParams)
    {
        var result = await _companyHRService.GetPendingCompanyHRsAsync(paginationParams);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize(Roles = AppRoles.CompanyHR)]
    public async Task<IActionResult> GetCurrentProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<CompanyHRResponseDto>.FailureResult("User not authenticated."));
        }

        var result = await _companyHRService.GetCurrentHRProfileAsync(userId);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPut("me")]
    [Authorize(Roles = AppRoles.CompanyHR)]
    public async Task<IActionResult> UpdateCurrentProfile([FromBody] UpdateCompanyHRProfileRequestDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<CompanyHRResponseDto>.FailureResult("User not authenticated."));
        }

        var result = await _companyHRService.UpdateCurrentHRProfileAsync(userId, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.CompanyHR}")]
    public async Task<IActionResult> GetCompanyHRById(int id)
    {
        var result = await _companyHRService.GetCompanyHRByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/approve")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> ApproveCompanyHR(int id)
    {
        var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _companyHRService.ApproveCompanyHRAsync(id, adminId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/reject")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> RejectCompanyHR(int id)
    {
        var result = await _companyHRService.RejectCompanyHRAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/suspend")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> SuspendCompanyHR(int id)
    {
        var result = await _companyHRService.SuspendCompanyHRAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
