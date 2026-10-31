
using LMS.Shared.DTOs.SubmissionDtos;
using LMS.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[Route("api/teacher/submissions")]
[Authorize(Roles = "Teacher")]
[ApiController]
public class TeacherSubmissionsController(
    IServiceManager serviceManager) : ApiControllerBase
{
    private readonly IServiceManager _serviceManager =
        serviceManager;

    [HttpGet]
    [ProducesResponseType(
       typeof(PagedResponse<TeacherAssignmentSubmissionDto>),
       StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task< ActionResult<PagedResponse<TeacherAssignmentSubmissionDto>>> GetSubmissions(
        [FromQuery] int? activityId,
        [FromQuery] string? status,
        [FromQuery] QueryParameters parameters)
    {
        var teacherId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        var result = await _serviceManager.SubmissionService
            .GetTeacherSubmissionsAsync(
                teacherId,
                activityId,
                status,
                parameters);

        return Ok(result);
    }

    [HttpGet("{submissionId:int}/file")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadFile(
        int submissionId)
    {
        var teacherId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(teacherId))
        {
            return Unauthorized();
        }

        var file = await _serviceManager.SubmissionService
            .DownloadSubmissionAsync(
                submissionId,
                teacherId);

        if (file is null)
        {
            return NotFound();
        }

        return File(
            file.Value.Content,
            "application/octet-stream",
            file.Value.FileName);
    }
}
