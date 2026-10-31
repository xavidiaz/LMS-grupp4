using LMS.Shared.DTOs.NotificationDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;

namespace LMS.Presentation.Controllers;

[Route("api/notifications")]
[ApiController]
[Authorize]
[Produces("application/json")]
public class NotificationsController(IServiceManager serviceManager) : ControllerBase
{
    private readonly IServiceManager _serviceManager = serviceManager;

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get notifications",
        Description = "Returns notifications belonging to the currently authenticated user.")]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Notifications retrieved successfully",
        typeof(IEnumerable<NotificationDto>))]
    [SwaggerResponse(
        StatusCodes.Status401Unauthorized,
        "User is not authenticated")]
    public async Task<ActionResult<IEnumerable<NotificationDto>>> GetNotifications()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var notifications =
            await _serviceManager.NotificationService.GetNotificationsAsync(userId);

        return Ok(notifications);
    }

    [HttpPatch("{notificationId:int}/read")]
    [SwaggerOperation(
        Summary = "Mark notification as read",
        Description = "Marks one of the currently authenticated user's notifications as read.")]
    [SwaggerResponse(
        StatusCodes.Status204NoContent,
        "Notification marked as read")]
    [SwaggerResponse(
        StatusCodes.Status401Unauthorized,
        "User is not authenticated")]
    [SwaggerResponse(
        StatusCodes.Status404NotFound,
        "Notification was not found")]
    public async Task<IActionResult> MarkAsRead(int notificationId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var updated =
            await _serviceManager.NotificationService.MarkAsReadAsync(
                notificationId,
                userId);

        if (!updated)
            return NotFound();

        return NoContent();
    }
}