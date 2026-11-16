using Taskist.Core.Domain.Users;
using Taskist.Core.Dtos.Notification;

namespace Taskist.Service.Messages;

public interface IUserNotificationService
{
    Task<int> NotifyAsync(UserNotificationRequest request);

    Task<IList<UserNotification>> GetRecentAsync(int userId, int take = 20);

    Task<int> GetUnreadCountAsync(int userId);

    Task<UserNotification?> MarkReadAsync(int userId, int id);

    Task MarkAllReadAsync(int userId);

    Task<bool> ExistsAsync(int userId, string dedupeKey, int? withinHours);
}
