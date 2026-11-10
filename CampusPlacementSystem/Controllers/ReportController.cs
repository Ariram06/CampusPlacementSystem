using System.Security.Claims;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("platform")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> GetPlatformSummary()
    {
        var result = await _reportService.GetPlatformSummaryAsync();
        return Ok(result);
    }

    [HttpGet("college")]
    [Authorize(Roles = $"{AppRoles.PlacementOfficer},{AppRoles.SuperAdmin}")]
    public async Task<IActionResult> GetCollegePlacementReport([FromQuery] int? collegeId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _reportService.GetCollegePlacementReportAsync(currentUserId, currentUserRole, collegeId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("company")]
    [Authorize(Roles = $"{AppRoles.CompanyHR},{AppRoles.SuperAdmin}")]
    public async Task<IActionResult> GetCompanyRecruitmentReport([FromQuery] int? companyId)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _reportService.GetCompanyRecruitmentReportAsync(currentUserId, currentUserRole, companyId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("student/me")]
    [Authorize(Roles = AppRoles.Student)]
    public async Task<IActionResult> GetStudentPlacementStatus()
    {
        var studentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;

        var result = await _reportService.GetStudentPlacementReportAsync(studentUserId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
