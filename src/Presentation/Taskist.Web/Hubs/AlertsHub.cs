using Taskist.Service.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Taskist.Web.Hubs;

[Authorize]
public class AlertsHub : Hub
{
    #region Fields

    protected readonly IRealtimeAlertNotifier _notifier;

    #endregion

    #region Ctor

    public AlertsHub(IRealtimeAlertNotifier notifier)
    {
        _notifier = notifier;
    }

    #endregion

    #region Methods

    public override async Task OnConnectedAsync()
    {
        if (int.TryParse(Context.UserIdentifier, out var userId))
            await _notifier.NotifyAsync(userId);

        await base.OnConnectedAsync();
    }

    #endregion
}
