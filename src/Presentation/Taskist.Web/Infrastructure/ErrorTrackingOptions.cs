namespace Taskist.Web.Infrastructure;

public class ErrorTrackingOptions
{
    #region Fields

    public const string SectionName = "Sentry";

    #endregion

    #region Properties

    public bool Enabled { get; set; }

    public string? Dsn { get; set; }

    public string? Environment { get; set; }

    public double TracesSampleRate { get; set; }

    public bool Debug { get; set; }

    public bool HasValidDsn =>
        Uri.TryCreate(Dsn?.Trim(), UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
        && !string.IsNullOrEmpty(uri.UserInfo)
        && uri.AbsolutePath.Trim('/').Length > 0;

    public bool IsActive => Enabled && HasValidDsn;

    public bool IsMisconfigured => Enabled && !HasValidDsn;

    public double EffectiveTracesSampleRate => double.IsFinite(TracesSampleRate) ? Math.Clamp(TracesSampleRate, 0, 1) : 0;

    #endregion
}
