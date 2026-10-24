namespace Taskist.Service.Logging;

public class LogRetentionService : ILogRetentionService
{
    private const int RetentionDays = 180;

    protected readonly IUserActivityService _activityService;
    protected readonly IAuditLogService _auditLogService;

    public LogRetentionService(IUserActivityService activityService,
        IAuditLogService auditLogService)
    {
        _activityService = activityService;
        _auditLogService = auditLogService;
    }

    public async Task PurgeAsync()
    {
        var cutoff = DateTime.Now.AddDays(-RetentionDays);

        await _activityService.DeleteOlderThanAsync(cutoff);
        await _auditLogService.DeleteOlderThanAsync(cutoff);
    }
}
