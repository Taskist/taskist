using Taskist.Core.Common;
using Taskist.Core.Domain.Logging;
using Taskist.Core.Domain.Users;
using Taskist.Data.Repository;

namespace Taskist.Service.Logging;

public class LogService : ILogService
{
    #region Fields

    protected readonly IRepository<Log> _logRepository;
    protected readonly IHttpHelper _httpHelper;

    #endregion

    #region Ctor

    public LogService(IRepository<Log> logRepository,
        IHttpHelper httpHelper)
    {
        _logRepository = logRepository;
        _httpHelper = httpHelper;
    }

    #endregion

    #region Methods

    public bool IsEnabled(LogLevel level)
    {
        return level switch
        {
            LogLevel.Debug => false,
            _ => true,
        };
    }

    public async Task<IPagedList<Log>> GetAllAsync(DateTime? fromUtc = null, DateTime? toUtc = null,
        string message = "", LogLevel? logLevel = null,
        int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var logs = await _logRepository.GetAllPagedAsync(query =>
        {
            if (fromUtc.HasValue)
                query = query.Where(l => fromUtc.Value <= l.CreatedOnUtc);
            if (toUtc.HasValue)
                query = query.Where(l => toUtc.Value >= l.CreatedOnUtc);
            if (logLevel.HasValue)
            {
                var logLevelId = (int)logLevel.Value;
                query = query.Where(l => logLevelId == l.LogLevelId);
            }

            if (!string.IsNullOrEmpty(message))
                query = query.Where(l => l.ShortMessage.Contains(message) || l.FullMessage.Contains(message));
            query = query.OrderByDescending(l => l.CreatedOnUtc);

            return query;
        }, pageIndex, pageSize);

        return logs;
    }

    public async Task<Log> GetByIdAsync(int logId)
    {
        return await _logRepository.GetByIdAsync(logId);
    }

    public async Task<IList<Log>> GetByIdsAsync(int[] logIds)
    {
        return await _logRepository.GetByIdsAsync(logIds);
    }

    private async Task<Log> InsertAsync(LogLevel logLevel, string shortMessage, string fullMessage = "", User user = null)
    {
        var log = new Log
        {
            LogLevel = logLevel,
            ShortMessage = shortMessage,
            FullMessage = fullMessage,
            IpAddress = _httpHelper.GetCurrentIpAddress(),
            UserId = user?.Id,
            PageUrl = _httpHelper.GetCurrentPageUrl(true),
            ReferrerUrl = _httpHelper.GetUrlReferrer(),
            CreatedOnUtc = DateTime.UtcNow
        };

        await _logRepository.InsertAsync(log);

        return log;
    }

    private Log Insert(LogLevel logLevel, string shortMessage, string fullMessage = "", User user = null)
    {
        var log = new Log
        {
            LogLevel = logLevel,
            ShortMessage = shortMessage,
            FullMessage = fullMessage,
            IpAddress = _httpHelper.GetCurrentIpAddress(),
            UserId = user?.Id,
            PageUrl = _httpHelper.GetCurrentPageUrl(true),
            ReferrerUrl = _httpHelper.GetUrlReferrer(),
            CreatedOnUtc = DateTime.UtcNow
        };

        _logRepository.Insert(log);

        return log;
    }

    public async Task InformationAsync(string message, Exception exception = null, User user = null)
    {
        if (exception is System.Threading.ThreadAbortException)
            return;

        if (IsEnabled(LogLevel.Information))
            await InsertAsync(LogLevel.Information, message, exception?.ToString() ?? string.Empty, user);
    }

    public void Information(string message, Exception exception = null, User user = null)
    {
        if (exception is System.Threading.ThreadAbortException)
            return;

        if (IsEnabled(LogLevel.Information))
            Insert(LogLevel.Information, message, exception?.ToString() ?? string.Empty, user);
    }

    public async Task WarningAsync(string message, Exception exception = null, User user = null)
    {
        if (exception is System.Threading.ThreadAbortException)
            return;

        if (IsEnabled(LogLevel.Warning))
            await InsertAsync(LogLevel.Warning, message, exception?.ToString() ?? string.Empty, user);
    }

    public void Warning(string message, Exception exception = null, User user = null)
    {
        if (exception is System.Threading.ThreadAbortException)
            return;

        if (IsEnabled(LogLevel.Warning))
            Insert(LogLevel.Warning, message, exception?.ToString() ?? string.Empty, user);
    }

    public async Task ErrorAsync(string message, Exception exception = null, User user = null)
    {
        if (exception is System.Threading.ThreadAbortException)
            return;

        if (IsEnabled(LogLevel.Error))
            await InsertAsync(LogLevel.Error, message, exception?.ToString() ?? string.Empty, user);
    }

    public async Task<int> ErrorAndGetIdAsync(string message, Exception exception = null, User user = null)
    {
        var log = new Log();

        if (exception is System.Threading.ThreadAbortException)
            return log.Id;

        if (IsEnabled(LogLevel.Error))
            log = await InsertAsync(LogLevel.Error, message, exception?.ToString() ?? string.Empty, user);

        return log.Id;
    }

    public void Error(string message, Exception exception = null, User user = null)
    {
        if (exception is System.Threading.ThreadAbortException)
            return;

        if (IsEnabled(LogLevel.Error))
            Insert(LogLevel.Error, message, exception?.ToString() ?? string.Empty, user);
    }

    public async Task DeleteAsync(Log log)
    {
        if (log == null)
            throw new ArgumentNullException(nameof(log));

        await _logRepository.DeleteAsync(log);
    }

    public async Task DeleteAsync(IList<Log> logs)
    {
        await _logRepository.DeleteAsync(logs);
    }

    public async Task ClearAsync(DateTime? olderThan = null)
    {
        if (olderThan == null)
            await _logRepository.TruncateAsync();
        else
            await _logRepository.DeleteAsync(p => p.CreatedOnUtc < olderThan.Value);
    }

    #endregion
}