using LMS.Shared.DTOs.UserDtos;
using LMS.Shared.Paging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;



namespace LMS.Presentation.Controllers
{

    [ApiController]
    [Authorize(Roles = "Teacher")]
    [Route("api/users")]
    public class UsersController : ApiControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/users
        [HttpGet]
        [SwaggerOperation(Summary = "Get users",
         Description = "Retrieves a paginated list of users based on the specified query parameters.")]
        [SwaggerResponse( StatusCodes.Status200OK, "Users retrieved successfully", typeof(PagedResponse<UserDto>))]
        [SwaggerResponse( StatusCodes.Status400BadRequest, "Invalid query parameters")]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResponse<UserDto>>> GetUsers(
            [FromQuery] QueryParameters query)
        {
            var users = await _userService.GetUsersAsync(query);
            return Ok(users);
        }

        // GET: api/users/{id}
        [HttpGet("{id}")]
        [SwaggerOperation(
        Summary = "Get user by ID",
        Description = "Retrieves a user by their unique identifier.")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<UserDto>> GetUser(string id)
        {
            var user = await _userService.GetUserAsync(id);

            return Ok(user);
        }

        // POST: api/users
        [HttpPost]
        [ProducesResponseType(typeof(CreateUserResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<CreateUserResponseDto>> CreateUser([FromBody] CreateUserDto dto)        
        {
            var result = await _userService.CreateUserAsync(dto);
               
            return CreatedAtAction(nameof(GetUser),new { id = result.User.Id }, result);
                
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserDto),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails),StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserDto>> UpdateUser(
            string id,
            [FromBody] UpdateUserDto dto)
        {
            var user =await _userService.UpdateUserAsync(id,dto);
            return Ok(user);
        }    
        [ProducesResponseType(StatusCodes.Status204NoContent)] 
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id) 
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
         

            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Unauthorized();
            }

            await _userService.DeleteUserAsync(id, currentUserId);

            return NoContent();
        }
        // POST: api/users/set-password
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [AllowAnonymous]
        [HttpPost("set-password")]
        public async Task<IActionResult> SetPassword(
            [FromBody] SetPasswordDto dto)
        {
            await _userService.SetPasswordAsync(dto);

            return NoContent();
        }
    }
}
