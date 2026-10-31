using Domain.Contracts;
using LMS.Shared.DTOs.NotificationDtos;
using Service.Contracts;

namespace LMS.Services;

public class NotificationService(IUnitOfWork uow) : INotificationService
{
    public async Task<IEnumerable<NotificationDto>> GetNotificationsAsync(string userId)
    {
        var notifications = await uow.NotificationRepository.GetNotificationsByUserIdAsync(userId);

        return notifications.Select(notification => new NotificationDto
        {
            Id = notification.Id,
            Message = notification.Message,
            CreatedAt = notification.CreatedAt,
            IsRead = notification.IsRead
        });
    }

    public async Task<bool> MarkAsReadAsync(int notificationId, string userId)
    {
        var notification = await uow.NotificationRepository.GetNotificationByIdAsync(notificationId, userId);

        if (notification is null)
            return false;

        if (notification.IsRead)
            return true;

        notification.IsRead = true;
        return await uow.CompleteAsync() > 0;
    }
}
