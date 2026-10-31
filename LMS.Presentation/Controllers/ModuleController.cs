using LMS.Shared.DTOs.Module;
using LMS.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace LMS.Presentation.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/Module")]
    public class ModuleController : ApiControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public ModuleController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        // GET: api/Module
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all modules",
            Description = "Retrieves a paginated list of modules.")]
        [ProducesResponseType(typeof(PagedResponse<ModuleDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll([FromQuery] QueryParameters parameters)
        {
            var modules = await _serviceManager.ModuleService.GetAllAsync(parameters);

            return Ok(modules);
        }

        // GET: api/Module/{id}
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Get module by ID",
            Description = "Retrieves a module by its unique identifier.")]
        [ProducesResponseType(typeof(ModuleDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(int id)
        {
            if (!await CanAccessModuleAsync(id))
                return Forbid();
            var module = await _serviceManager.ModuleService.GetByIdAsync(id);

            return Ok(module);
        }

        // GET: api/Module/{id}/course
        [HttpGet("{id:int}/course")]
        [SwaggerOperation(
            Summary = "Get module with course",
            Description = "Retrieves a module together with its associated course.")]
        [ProducesResponseType(typeof(ModuleDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetByIdWithCourse(int id)
        {
            var module = await _serviceManager.ModuleService.GetByIdWithCourseAsync(id);

            return Ok(module);
        }

        // GET: api/Module/course/{courseId}
        [HttpGet("course/{courseId:int}")]
        [SwaggerOperation(
            Summary = "Get modules by course",
            Description = "Retrieves modules belonging to a course in chronological order.")]
        [ProducesResponseType(typeof(PagedResponse<ModuleDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetByCourseId(int courseId)
        {
            var modules = await _serviceManager.ModuleService.GetByCourseIdAsync(courseId);

            return Ok(modules);
        }

        // GET: api/Module/{courseId}/overlapping-dates
        [HttpGet("{courseId:int}/overlapping-dates")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Check overlapping module dates",
            Description = "Checks whether the specified date range overlaps an existing module in a course.")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> HasOverlappingDates(
            int courseId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] int? excludeModuleId = null)
        {
            var isOverlapping =
                await _serviceManager.ModuleService.HasOverlappingDatesAsync(courseId, startDate, endDate, excludeModuleId);

            return Ok(new { isOverlapping });
        }

        // POST: api/Module
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Create a module",
            Description = "Creates a module after validating course dates and checking for overlaps.")]
        [ProducesResponseType(typeof(ModuleDto),
            StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateModuleDto createModuleDto)
        {
            var module = await _serviceManager.ModuleService.CreateModuleAsync(createModuleDto);

            return CreatedAtAction(nameof(GetById), new { id = module.Id }, module);
        }

        // PUT: api/Module/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Update a module",
            Description = "Updates a module after validating its dates and checking for overlaps.")]
        [ProducesResponseType(typeof(ModuleDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update( int id,[FromBody] UpdateModuleDto updateModuleDto)
        {
            var module = await _serviceManager.ModuleService.UpdateModuleAsync(id,updateModuleDto);
      
            return Ok(module);
        }

        // DELETE: api/Module/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Delete a module",
            Description = "Deletes a module by its unique identifier.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails),
            StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(int id)
        {
            await _serviceManager.ModuleService.DeleteModuleAsync(id);

            return NoContent();
        }

        // Teachers can access all courses, students only their own
        private async Task<bool> CanAccessModuleAsync(int? moduleId)
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
    }
}
