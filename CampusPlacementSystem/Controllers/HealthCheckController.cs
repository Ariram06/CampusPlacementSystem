using CampusPlacementSystem.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthCheckController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(ApiResponse<object>.SuccessResult(new
        {
            Status = "Healthy",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            Timestamp = DateTime.UtcNow,
            Framework = ".NET 10.0"
        }, "Campus Placement Management System API is running."));
    }
}
