using System.Security.Claims;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Company;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterCompany([FromBody] CreateCompanyRequestDto request)
    {
        var result = await _companyService.RegisterCompanyAsync(request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetCompanyById), new { id = result.Data!.CompanyId }, result);
    }

    [HttpGet]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> GetAllCompanies([FromQuery] PaginationParams paginationParams, [FromQuery] CompanyStatus? status = null)
    {
        var result = await _companyService.GetAllCompaniesAsync(paginationParams, status);
        return Ok(result);
    }

    [HttpGet("pending")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> GetPendingCompanies([FromQuery] PaginationParams paginationParams)
    {
        var result = await _companyService.GetPendingCompaniesAsync(paginationParams);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.CompanyHR}")]
    public async Task<IActionResult> GetCompanyById(int id)
    {
        var result = await _companyService.GetCompanyByIdAsync(id);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.CompanyHR}")]
    public async Task<IActionResult> UpdateCompany(int id, [FromBody] UpdateCompanyRequestDto request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _companyService.UpdateCompanyAsync(id, request, userId, role);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/approve")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> ApproveCompany(int id)
    {
        var result = await _companyService.ApproveCompanyAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/reject")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> RejectCompany(int id)
    {
        var result = await _companyService.RejectCompanyAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/suspend")]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> SuspendCompany(int id)
    {
        var result = await _companyService.SuspendCompanyAsync(id);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
