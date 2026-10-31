using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class NotificationRepository(ApplicationDbContext context) : RepositoryBase<Notification>(context), INotificationRepository
{
    public async Task<IEnumerable<Notification>> GetNotificationsByUserIdAsync(string userId) =>
        await FindByCondition(n => n.UserId == userId)
        .OrderByDescending(n => n.CreatedAt)
        .ToListAsync();

    // trackChanges: true – MarkAsReadAsync changes IsRead and saves it
    public async Task<Notification?> GetNotificationByIdAsync(int notificationId, string userId) =>
        await FindByCondition(n => n.Id == notificationId && n.UserId == userId, trackChanges: true)
        .FirstOrDefaultAsync();
}
