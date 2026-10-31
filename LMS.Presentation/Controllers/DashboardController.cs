
using LMS.Shared.DTOs.DashboardDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IServiceManager _serviceManager;

    public DashboardController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    // US12 - Student Dashboard
    [HttpGet("student")]
    [Authorize(Roles = "Student")]
    public async Task<ActionResult<DashboardDto>> GetStudentDashboard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var dashboard = await _serviceManager.DashboardService
            .GetStudentDashboardAsync(userId);

        if (dashboard is null)
            return NotFound();

        return Ok(dashboard);
    }

    // US13 - Teacher Dashboard
    [HttpGet("teacher")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<TeacherDashboardDto>> GetTeacherDashboard()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var dashboard = await _serviceManager.DashboardService
            .GetTeacherDashboardAsync(userId);

        if (dashboard is null)
            return NotFound();

        return Ok(dashboard);
    }
}
