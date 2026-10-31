using Domain.Models.Entities;

namespace Domain.Contracts;

public interface INotificationRepository : IRepositoryBase<Notification>
{
    Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(string userId);
    Task<Notification?> GetNotificationByIdAsync(int notificationId, string userId);
}
