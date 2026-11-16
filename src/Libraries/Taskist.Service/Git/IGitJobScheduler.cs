namespace Taskist.Service.Git;

public interface IGitJobScheduler
{
    Task EnqueueWebhookAsync(int deliveryId);

    Task EnqueueIssuePushAsync(int backlogId);
}
