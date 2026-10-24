using Microsoft.AspNetCore.SignalR;
using Taskist.Service.Common;
using Taskist.Service.Messages;
using Taskist.Service.Users;
using Taskist.Service.WorkItems;
using Taskist.Web.Hubs;

namespace Taskist.Web.Infrastructure.Realtime;

public class RealtimeAlertNotifier : IRealtimeAlertNotifier
{
    #region Fields

    protected readonly IHubContext<AlertsHub> _hub;
    protected readonly IServiceScopeFactory _scopeFactory;

    #endregion

    #region Ctor

    public RealtimeAlertNotifier(IHubContext<AlertsHub> hub, IServiceScopeFactory scopeFactory)
    {
        _hub = hub;
        _scopeFactory = scopeFactory;
    }

    #endregion

    #region Methods

    public async Task NotifyAsync(params int[] userIds)
    {
        var ids = userIds.Where(x => x > 0).Distinct().ToArray();
        if (ids.Length == 0)
            return;

        using var scope = _scopeFactory.CreateScope();
        var backlogService = scope.ServiceProvider.GetRequiredService<IBacklogService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var userNotificationService = scope.ServiceProvider.GetRequiredService<IUserNotificationService>();

        foreach (var id in ids)
        {
            try
            {
                var accessibleProjectIds = (await userService.GetAllAccessibleProjects(id)).Select(p => p.Id).ToArray();
                var due = await backlogService.GetDueTasksAsync(id, accessibleProjectIds);
                var overdue = due.GetValueOrDefault("Overdue", 0);

                await _hub.Clients.User(id.ToString()).SendAsync("alerts", new
                {
                    overdueTasks = overdue,
                    unreadNotifications = await userNotificationService.GetUnreadCountAsync(id)
                });
            }
            catch
            {
            }
        }
    }

    #endregion
}
