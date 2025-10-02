using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Logging;

public class Log : BaseEntity
{
    public int LogLevelId { get; set; }

    public string ShortMessage { get; set; }

    public string FullMessage { get; set; }

    public string? IpAddress { get; set; }

    public int? UserId { get; set; }

    public string? PageUrl { get; set; }

    public string? ReferrerUrl { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public LogLevel LogLevel
    {
        get => (LogLevel)LogLevelId;
        set => LogLevelId = (int)value;
    }
}
