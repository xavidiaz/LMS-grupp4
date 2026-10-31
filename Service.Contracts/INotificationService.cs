using LMS.Shared.DTOs.NotificationDtos;

namespace Service.Contracts;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetNotificationsAsync(string userId);
    Task<bool> MarkAsReadAsync(int notificationId, string userId);
}