namespace Taskist.Service.Logging;

public interface ILogRetentionService
{
    Task PurgeAsync();
}
