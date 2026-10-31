using System.Security.Claims;
using LMS.Shared.DTOs.DocumentDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace LMS.Presentation.Controllers;

[Route("api/documents")]
[Authorize]
[ApiController]
public class DocumentController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet("course/{courseId:int}")]
    [Authorize(Roles = "Teacher,Student")]
    [SwaggerOperation(Summary = "Course documents", Description = "Documents for a course and its modules")]
    [SwaggerResponse(StatusCodes.Status200OK, "successful", typeof(IEnumerable<DocumentDto>))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "forbidden")]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetByCourse(int courseId)
    {
        if (!await CanAccessCourseDocumentAsync(courseId))
            return Forbid();

        var documents = await _serviceManager.DocumentService.GetByCourseIdAsync(courseId);
        return Ok(documents);
    }

    [HttpGet("module/{moduleId:int}")]
    [Authorize(Roles = "Student,Teacher")]
    [SwaggerOperation(Summary = "Module documents", Description = "Documents for a module")]
    [SwaggerResponse(StatusCodes.Status200OK, "successful", typeof(IEnumerable<DocumentDto>))]
    public async Task<ActionResult<IEnumerable<DocumentDto>>> GetByModule(int moduleId)
    {
        if (!await CanAccessModuleDocumentAsync(moduleId))
            return Forbid();
        var documents = await _serviceManager.DocumentService.GetByModuleIdAsync(moduleId);
        return Ok(documents);
    }

    [HttpGet("{id:int}/download")]
    [Authorize(Roles = "Teacher,Student")]
    [SwaggerOperation(Summary = "Download document", Description = "Returns the file")]
    [SwaggerResponse(StatusCodes.Status200OK, "successful")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "forbidden")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "not found")]
    public async Task<IActionResult> Download(int id)
    {
        var document = await _serviceManager.DocumentService.GetDocumentAsync(id);
        if (document is null)
            return NotFound();

        if (!await CanDownloadDocumentAsync(id))
            return Forbid();

        var file = await _serviceManager.DocumentService.DownloadAsync(id);
        return file is null
            ? NotFound()
            : File(file.Content, file.ContentType, file.FileName);
    }

    [HttpPost]
    [Authorize(Roles = "Teacher,Student")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    [SwaggerOperation(Summary = "Upload document", Description = "Max 10 MB. PDF, DOCX, PPTX, XLSX, TXT, JPG, PNG")]
    [SwaggerResponse(StatusCodes.Status200OK, "successful", typeof(DocumentDto))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "invalid file")]
    public async Task<ActionResult<DocumentDto>> Upload(
        IFormFile file,
        [FromForm] int? courseId,
        [FromForm] int? moduleId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Forbid();
        if (!await CanUploadDocumentAsync(courseId, moduleId))
            return Forbid();

        var dto = new UploadDocumentDto(file.FileName, file.ContentType, file.Length, courseId, moduleId);

        await using var stream = file.OpenReadStream();
        var result = await _serviceManager.DocumentService.UploadAsync(dto, stream, userId);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Teacher")]
    [SwaggerOperation(Summary = "Delete document", Description = "Deletes the document and its file")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "deleted")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "not found")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _serviceManager.DocumentService.DeleteDocumentAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    // Teachers can access all courses, students only their own
    private async Task<bool> CanAccessCourseDocumentAsync(int? courseId)
    {
        if (User.IsInRole("Teacher"))
            return true;

        if (courseId is null)
            return false;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return false;
        var user = await _serviceManager.UserService.GetUserAsync(userId);
        return user.CourseId == courseId;
    }

    // Teachers can access all courses, students only their own
    private async Task<bool> CanAccessModuleDocumentAsync(int? moduleId)
    {
        if (User.IsInRole("Teacher"))
            return true;

        if (moduleId is null)
            return false;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return false;
        var user = await _serviceManager.UserService.GetUserAsync(userId);
        var module = await _serviceManager.ModuleService.GetByIdAsync((int)moduleId);
        return module!.CourseId == user.CourseId;
    }

    private async Task<bool> CanDownloadDocumentAsync(int documentId)
    {
        if (User.IsInRole("Teacher"))
            return true;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return false;
        var user = await _serviceManager.UserService.GetUserAsync(userId);
        if (user.CourseId is null)
            return false;
        return await _serviceManager.DocumentService.IsInCourseAsync((int)user.CourseId, documentId);
    }

    private async Task<bool> CanUploadDocumentAsync(int? courseId, int? moduleId)
    {
        if (courseId != null && moduleId != null)
            return false;
        if (User.IsInRole("Teacher"))
            return true;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return false;
        var user = await _serviceManager.UserService.GetUserAsync(userId);
        if (user.CourseId is null)
            return false;
        if (user.CourseId == courseId)
            return true;
        if (moduleId is null)
            return false;
        var module = await _serviceManager.ModuleService.GetByIdAsync((int)moduleId);
        return module!.CourseId == user.CourseId;
    }
}
