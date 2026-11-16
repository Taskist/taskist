using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Taskist.Core.Configuration;

namespace Taskist.Service.Git;

public class GitJobScheduler : IGitJobScheduler
{
    #region Fields

    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly BackgroundJobMode _backgroundJobMode;
    private readonly IServiceProvider _serviceProvider;

    #endregion

    #region Ctor

    public GitJobScheduler(IBackgroundJobClient backgroundJobClient, BackgroundJobMode backgroundJobMode, IServiceProvider serviceProvider)
    {
        _backgroundJobClient = backgroundJobClient;
        _backgroundJobMode = backgroundJobMode;
        _serviceProvider = serviceProvider;
    }

    #endregion

    #region Methods

    public async Task EnqueueWebhookAsync(int deliveryId)
    {
        if (_backgroundJobMode.ServerEnabled)
        {
            _backgroundJobClient.Enqueue<IGitWebhookService>(svc => svc.ProcessAsync(deliveryId));
            return;
        }

        await _serviceProvider.GetRequiredService<IGitWebhookService>().ProcessAsync(deliveryId);
    }

    public async Task EnqueueIssuePushAsync(int backlogId)
    {
        if (_backgroundJobMode.ServerEnabled)
        {
            _backgroundJobClient.Enqueue<Issues.IIssueSyncService>(svc => svc.PushAsync(backlogId));
            return;
        }

        try
        {
            await _serviceProvider.GetRequiredService<Issues.IIssueSyncService>().PushAsync(backlogId);
        }
        catch (Exception)
        {
        }
    }

    #endregion
}
