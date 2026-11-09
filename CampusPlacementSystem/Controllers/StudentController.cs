using System.Security.Claims;
using CampusPlacementSystem.DTOs.Common;
using CampusPlacementSystem.DTOs.Student;
using CampusPlacementSystem.Enums;
using CampusPlacementSystem.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StudentController : ControllerBase
{
    private readonly IStudentService _studentService;

    public StudentController(IStudentService studentService)
    {
        _studentService = studentService;
    }

    [HttpPost("enroll")]
    [Authorize(Roles = AppRoles.PlacementOfficer)]
    public async Task<IActionResult> EnrollStudent([FromBody] EnrollStudentRequestDto request)
    {
        var officerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _studentService.EnrollStudentAsync(request, officerUserId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetStudentById), new { id = result.Data!.StudentId }, result);
    }

    [HttpPost("bulk-enroll")]
    [Authorize(Roles = AppRoles.PlacementOfficer)]
    public async Task<IActionResult> BulkEnrollStudents(IFormFile file)
    {
        var officerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _studentService.BulkEnrollStudentsAsync(file, officerUserId);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.PlacementOfficer}")]
    public async Task<IActionResult> GetAllStudents([FromQuery] StudentFilterParams filterParams)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _studentService.GetAllStudentsAsync(filterParams, currentUserId, currentUserRole);
        return Ok(result);
    }

    [HttpGet("pending")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.PlacementOfficer}")]
    public async Task<IActionResult> GetPendingStudents([FromQuery] PaginationParams paginationParams)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _studentService.GetPendingStudentsAsync(paginationParams, currentUserId, currentUserRole);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize(Roles = AppRoles.Student)]
    public async Task<IActionResult> GetCurrentProfile()
    {
        var studentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(studentUserId))
        {
            return Unauthorized(ApiResponse<StudentResponseDto>.FailureResult("User not authenticated."));
        }

        var result = await _studentService.GetCurrentStudentProfileAsync(studentUserId);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPut("me")]
    [Authorize(Roles = AppRoles.Student)]
    public async Task<IActionResult> UpdateCurrentProfile([FromBody] UpdateStudentProfileRequestDto request)
    {
        var studentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(studentUserId))
        {
            return Unauthorized(ApiResponse<StudentResponseDto>.FailureResult("User not authenticated."));
        }

        var result = await _studentService.UpdateStudentProfileAsync(studentUserId, request);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.PlacementOfficer},{AppRoles.Student}")]
    public async Task<IActionResult> GetStudentById(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _studentService.GetStudentByIdAsync(id, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/approve")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.PlacementOfficer}")]
    public async Task<IActionResult> ApproveStudent(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _studentService.ApproveStudentAsync(id, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/reject")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.PlacementOfficer}")]
    public async Task<IActionResult> RejectStudent(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _studentService.RejectStudentAsync(id, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPatch("{id:int}/suspend")]
    [Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.PlacementOfficer}")]
    public async Task<IActionResult> SuspendStudent(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var currentUserRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _studentService.SuspendStudentAsync(id, currentUserId, currentUserRole);
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}
