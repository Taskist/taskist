using Taskist.Core.Common;
using Taskist.Core.Domain.Logging;
using Taskist.Core.Domain.Users;

namespace Taskist.Service.Logging;

public partial interface ILogService
{
    bool IsEnabled(LogLevel level);

    Task<IPagedList<Log>> GetAllAsync(DateTime? fromUtc = null, DateTime? toUtc = null,
        string message = "", LogLevel? logLevel = null,
        int pageIndex = 0, int pageSize = int.MaxValue);

    Task<Log> GetByIdAsync(int logId);

    Task<IList<Log>> GetByIdsAsync(int[] logIds);

    Task InformationAsync(string message, Exception exception = null, User user = null);

    void Information(string message, Exception exception = null, User user = null);

    Task WarningAsync(string message, Exception exception = null, User user = null);

    void Warning(string message, Exception exception = null, User user = null);

    Task ErrorAsync(string message, Exception exception = null, User user = null);

    Task<int> ErrorAndGetIdAsync(string message, Exception exception = null, User user = null);

    void Error(string message, Exception exception = null, User user = null);

    Task DeleteAsync(Log log);

    Task DeleteAsync(IList<Log> logs);

    Task ClearAsync(DateTime? olderThan = null);
}