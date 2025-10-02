namespace Taskist.Core.Caching;

public interface ILocker
{
    Task<bool> PerformActionWithLock(string resource, TimeSpan expirationTime, Action action);
}
