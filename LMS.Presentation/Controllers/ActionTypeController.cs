using LMS.Shared.DTOs.ActionType;
using LMS.Shared.DTOs.Activity;
using LMS.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Presentation.Controllers
{

    [ApiController]
    [Authorize]
    [Route("api/ActionType")]
    public class ActionTypeController : ApiControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public ActionTypeController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        // GET: api/ActionType
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get all action types",
            Description = "Retrieves a paginated list of action types.")]
        [ProducesResponseType(
            typeof(PagedResponse<ActionTypeDto>),
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
            var actionTypes =
                await _serviceManager.ActionTypeService.GetAllAsync(parameters);

            return Ok(actionTypes);
        }

        // GET: api/ActionType/activity/{activityId}
        [HttpGet("activity/{activityId:int}")]
        [SwaggerOperation(
            Summary = "Get action type by Activity",
            Description = "Retrieves the action type for a specific activity.")]
        [ProducesResponseType(
            typeof(PagedResponse<ActionTypeDto>),
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
        public async Task<IActionResult> GetByActivityId(
            int activityId )
            
        {
            var actionTypes =
                await _serviceManager.ActionTypeService.GetByActivityIdAsync(
                    activityId);

            return Ok(actionTypes);
        }
    }
}
