using Taskist.Web.Infrastructure;

namespace Taskist.Tests.Infrastructure;

public class ErrorTrackingOptionsTests
{
    #region Fields

    private const string ValidDsn = "https://0123456789abcdef@o123456.ingest.sentry.io/4501234567";

    #endregion

    #region Tests

    [Fact]
    public void Defaults_AreOff()
    {
        var options = new ErrorTrackingOptions();

        Assert.False(options.IsActive);
        Assert.False(options.IsMisconfigured);
        Assert.Equal(0, options.EffectiveTracesSampleRate);
    }

    [Fact]
    public void ValidDsn_ButDisabled_IsNotActive()
    {
        var options = new ErrorTrackingOptions { Enabled = false, Dsn = ValidDsn };

        Assert.False(options.IsActive);
        Assert.False(options.IsMisconfigured);
    }

    [Fact]
    public void Enabled_WithValidDsn_IsActive()
    {
        var options = new ErrorTrackingOptions { Enabled = true, Dsn = "  " + ValidDsn + " " };

        Assert.True(options.IsActive);
        Assert.False(options.IsMisconfigured);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("https://YOUR_SENTRY_DSN_HERE")]
    [InlineData("not a url")]
    [InlineData("https://o123456.ingest.sentry.io/4501234567")]
    [InlineData("https://key@o123456.ingest.sentry.io/")]
    [InlineData("ftp://key@o123456.ingest.sentry.io/4501234567")]
    public void Enabled_WithMissingOrInvalidDsn_IsMisconfigured(string? dsn)
    {
        var options = new ErrorTrackingOptions { Enabled = true, Dsn = dsn };

        Assert.False(options.IsActive);
        Assert.True(options.IsMisconfigured);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0.25, 0.25)]
    [InlineData(5, 1)]
    [InlineData(double.NaN, 0)]
    [InlineData(double.PositiveInfinity, 0)]
    public void TracesSampleRate_IsClampedToZeroToOne(double configured, double expected)
    {
        var options = new ErrorTrackingOptions { TracesSampleRate = configured };

        Assert.Equal(expected, options.EffectiveTracesSampleRate);
    }

    #endregion
}
