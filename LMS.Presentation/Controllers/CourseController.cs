using System.Security.Claims;
using LMS.Shared.DTOs.CourseDtos;
using LMS.Shared.DTOs.UserDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;

[Route("api/courses")]
[Authorize]
[ApiController]
[Consumes("application/json")]
[Produces("application/json")]
public class CourseController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet]
    [Authorize(Roles = "Teacher")]
    [SwaggerOperation(Summary = "Get all courses", Description = "Returns all courses for teachers")]
    [SwaggerResponse(StatusCodes.Status200OK, "Successful", typeof(List<CourseDto>))]
    public async Task<ActionResult<List<CourseDto>>> GetCourses()
    {
        var courses = await _serviceManager.CourseService.GetCoursesAsync();
        return Ok(courses);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Teacher,Student")]
    [SwaggerOperation(Summary = "Course", Description = "Course desc")]
    [SwaggerResponse(StatusCodes.Status200OK, "successful", typeof(CourseDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "unauthorized")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "forbidden")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "not found")]
    public async Task<ActionResult<CourseDto>> GetCourse(int id)
    {
        if (User.IsInRole("Student"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized();

            var user = await _serviceManager.UserService.GetUserAsync(userId);
            if (user.CourseId != id)
                return Forbid();
        }

        var course = await _serviceManager.CourseService.GetCourseAsync(id);
        return course is null ? NotFound() : Ok(course);

    }

    [HttpGet("my-course")]
    [Authorize(Roles = "Student")]
    [SwaggerOperation(Summary = "My course", Description = "Returns the course of the logged-in student")]
    [SwaggerResponse(StatusCodes.Status200OK, "successful", typeof(CourseDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "unauthorized")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "forbidden")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "not found")]
    public async Task<ActionResult<CourseDto>> GetMyCourse()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var user = await _serviceManager.UserService.GetUserAsync(userId);
        if (!user.CourseId.HasValue)
            return NotFound();

        var course = await _serviceManager.CourseService.GetCourseAsync(user.CourseId.Value);
        return course is null ? NotFound() : Ok(course);
    }

    [HttpGet("my-course/participants")]
    [Authorize(Roles = "Student")]
    [SwaggerOperation(Summary = "My course participants", Description = "Returns theparticipants of the logged-in student's course")]
    [SwaggerResponse(StatusCodes.Status200OK, "successful", typeof(IEnumerable<CourseParticipantDto>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "unauthorized")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "forbidden")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "not found")]
    public async Task<ActionResult<IEnumerable<CourseParticipantDto>>> GetMyCourseParticipants()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var user = await _serviceManager.UserService.GetUserAsync(userId);
        if (!user.CourseId.HasValue)
            return NotFound();

        var participants = await
        _serviceManager.UserService.GetCourseParticipantsAsync(user.CourseId.Value);
        return Ok(participants);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [SwaggerOperation(Summary = "New course", Description = "New course desc")]
    [SwaggerResponse(StatusCodes.Status200OK, "successful", typeof(CourseDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "unauthorized")]
    public async Task<ActionResult<CourseDto>> NewCourse(NewCourseDto course)
    {
        var ret = await _serviceManager.CourseService.NewCourseAsync(course);
        return Ok(ret);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Teacher")]
    [SwaggerOperation(
    Summary = "Delete course",
    Description = "Delete a course if no students or documents are associated with it.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Course deleted successfully")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Unauthorized")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Forbidden")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Course not found")]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Course cannot be deleted because it has associated students or documents")]
    public async Task<ActionResult<bool>> DeleteCourse(int id)
    {
        var ret = await _serviceManager.CourseService.DeleteCourseAsync(id);
        return Ok(ret);
    }

    [HttpPatch]
    [Authorize(Roles = "Teacher")]
    [SwaggerOperation(Summary = "Patch course", Description = "Patch course desc")]
    [SwaggerResponse(StatusCodes.Status200OK, "successful", typeof(CourseDto))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "unauthorized")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "forbidden")]
    public async Task<ActionResult<CourseDto>> PatchCourse(PatchCourseDto course)
    {
        var ret = await _serviceManager.CourseService.PatchCourseAsync(course);
        return Ok(ret);
    }
}
