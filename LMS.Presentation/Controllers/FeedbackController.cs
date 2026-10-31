using System.Security.Claims;
using LMS.Shared.DTOs.FeedbackDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;

[Route("api/submissions/{submissionId:int}/feedback")]
[Authorize]
[ApiController]
public class FeedbackController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpPost]
    [Authorize(Roles = "Teacher")]
    [SwaggerOperation(Summary = "Give feedback", Description = "Teacher gives feedback and grade (0–10) on a submission")]
    [SwaggerResponse(StatusCodes.Status200OK, "successful", typeof(FeedbackDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "invalid input")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "submission not found")]
    public async Task<ActionResult<FeedbackDto>> Create(int submissionId, [FromBody] CreateFeedbackDto dto)
    {
        var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(teacherId))
            return Unauthorized();

        var result = await _serviceManager.FeedbackService.CreateAsync(submissionId, dto, teacherId);
        return Ok(result);
    }

    [HttpGet]
    [Authorize(Roles = "Teacher")]
    [SwaggerOperation(Summary = "Feedback on submission", Description = "All feedback on a submission, newest first")]
    [SwaggerResponse(StatusCodes.Status200OK, "successful", typeof(IEnumerable<FeedbackDto>))]
    public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetBySubmission(int submissionId)
    {
        var result = await _serviceManager.FeedbackService.GetBySubmissionIdAsync(submissionId);
        return Ok(result);
    }

    [HttpGet("mine")]
    [Authorize(Roles = "Student")]
    [SwaggerOperation(Summary = "My feedback", Description = "Feedback on the logged-in student's own submission")]
    [SwaggerResponse(StatusCodes.Status200OK, "successful", typeof(IEnumerable<FeedbackDto>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "submission not found or not yours")]
    public async Task<ActionResult<IEnumerable<FeedbackDto>>> GetMine(int submissionId)
    {
        var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(studentId))
            return Unauthorized();

        var result = await _serviceManager.FeedbackService.GetMyFeedbackAsync(submissionId, studentId);
        return Ok(result);
    }
}
