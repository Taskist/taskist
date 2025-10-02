namespace Taskist.Service.Messages;

public partial interface INotificationService
{
    void Notification(NotifyType type, string message, bool encode = true);

    void SuccessNotification(string message, bool encode = true);

    void WarningNotification(string message, bool encode = true);

    void ErrorNotification(string message, bool encode = true);

    Task ErrorNotificationAsync(Exception exception, bool logException = true);
}
