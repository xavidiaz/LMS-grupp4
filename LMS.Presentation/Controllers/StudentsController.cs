using LMS.Shared.DTOs.UserDtos;
using LMS.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;

[Route("api/students")]
[Authorize(Roles = "Teacher")]
[ApiController]
public class StudentsController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet("{id}/overview")]
    [SwaggerOperation(Summary = "Student overview", Description = "The student's course, modules and assignments with status and feedback")]
    [SwaggerResponse(StatusCodes.Status200OK, "successful", typeof(StudentOverviewDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "unauthorized")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "forbidden")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "not found")]
    public async Task<ActionResult<StudentOverviewDto>> GetOverview(string id)
    {
        // Throws NotFoundException (404) when the user does not exist
        var student = await _serviceManager.UserService.GetUserAsync(id, includeCourse: true);
        if (student.Role != "Student")
            return NotFound();

        var modules = student.CourseId is int courseId
            ? (await _serviceManager.ModuleService.GetByCourseIdAsync(courseId))?.Items
                .OrderBy(m => m.StartDate)
                .ToList() ?? []
            : [];

        var now = DateTime.Now;
        var currentModule = modules.FirstOrDefault(m => m.StartDate <= now && now < m.EndDate);

        // 100 is the API's largest page size
        var assignments = await _serviceManager.ActivityService.GetStudentAssignmentsAsync(
            id, new QueryParameters { PageNumber = 1, PageSize = 100 });

        return Ok(new StudentOverviewDto
        {
            Student = student,
            Modules = modules,
            CurrentModuleId = currentModule?.Id,
            Assignments = assignments?.Items.OrderBy(a => a.Deadline).ToList() ?? []
        });
    }
}
