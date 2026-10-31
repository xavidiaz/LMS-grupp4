using LMS.Shared.DTOs.Activity;
using LMS.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LMS.Presentation.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/Activity")]
    public class ActivityController : ApiControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public ActivityController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        // GET: api/Activity
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all activities",
            Description = "Retrieves a paginated list of activities.")]
        [ProducesResponseType(
            typeof(PagedResponse<ActivityDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAll(
            [FromQuery] QueryParameters parameters)
        {
            var activities =
                await _serviceManager.ActivityService.GetAllAsync(parameters);

            return Ok(activities);
        }

        // GET: api/Activity/module/{moduleId}
        [HttpGet("module/{moduleId:int}")]
        [SwaggerOperation(
            Summary = "Get activities by module",
            Description = "Retrieves paginated activities for a module.")]
        [ProducesResponseType(
            typeof(PagedResponse<ActivityDto>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetByModuleId(
            int moduleId,
            [FromQuery] QueryParameters parameters)
        {
            var activities =
                await _serviceManager.ActivityService.GetByModuleIdAsync(
                    moduleId, parameters);

            return Ok(activities);
        }

        // GET: api/Activity/{id}
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Get activity by ID",
            Description = "Retrieves an activity by its unique identifier.")]
        [ProducesResponseType(
            typeof(ActivityDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetById(int id)
        {
            var activity = await _serviceManager.ActivityService.GetByIdAsync(id);

            return Ok(activity);
        }

        // POST: api/Activity
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Create an activity",
            Description = "Creates a new activity.")]
        [ProducesResponseType(
            typeof(ActivityDto),
            StatusCodes.Status201Created)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status409Conflict)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(
            [FromBody] CreateActivityDto createActivityDto)
        {
            var activity =
                await _serviceManager.ActivityService.CreateActivityAsync(
                    createActivityDto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = activity.Id },
                activity);
        }

        // PUT: api/Activity/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Update an activity",
            Description = "Updates an existing activity.")]
        [ProducesResponseType(
            typeof(ActivityDto),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status409Conflict)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(
            int id,
            [FromBody] UpdateActivityDto updateActivityDto)
        {
            var activity =
                await _serviceManager.ActivityService.UpdateActivityAsync(
                    id,
                    updateActivityDto);

            return Ok(activity);
        }

        // DELETE: api/Activity/{id}
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Teacher")]
        [SwaggerOperation(
            Summary = "Delete an activity",
            Description = "Deletes an activity by its unique identifier.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(int id)
        {
            await _serviceManager.ActivityService.DeleteActivityAsync(id);

            return NoContent();
        }

        [HttpGet("my-assignments")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ProblemDetails),
            StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetMyAssignments(
        [FromQuery] QueryParameters parameters)
        {
            var userId = User.FindFirstValue(
                System.Security.Claims.ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var result = await _serviceManager.ActivityService.GetStudentAssignmentsAsync(
                userId,
                parameters);

            return Ok(result);
        }
    }
}
