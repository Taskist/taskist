using Hangfire;
using Taskist.Service.Git;
using Taskist.Service.Git.Issues;
using Taskist.Service.Logging;

namespace Taskist.Web.Infrastructure;

public static class RecurringJobScheduler
{
    public static void RegisterLogPurge()
    {
        RecurringJob.AddOrUpdate<ILogRetentionService>(
            "log-retention",
            svc => svc.PurgeAsync(),
            "0 3 * * *",
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Local,
                MisfireHandling = MisfireHandlingMode.Ignorable
            });
    }

    public static void RegisterGitJobs()
    {
        RecurringJob.AddOrUpdate<IIssueSyncService>(
            "git-issue-sync",
            svc => svc.RunScheduledAsync(),
            "*/5 * * * *",
            new RecurringJobOptions { MisfireHandling = MisfireHandlingMode.Ignorable });

        RecurringJob.AddOrUpdate<IGitWebhookService>(
            "git-webhook-purge",
            svc => svc.PurgeAsync(14),
            "30 3 * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Local, MisfireHandling = MisfireHandlingMode.Ignorable });
    }
}
