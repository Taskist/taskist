namespace Taskist.Service.Common;

public interface IRealtimeAlertNotifier
{
    Task NotifyAsync(params int[] userIds);
}
