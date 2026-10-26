using System.Text.RegularExpressions;
using Sentry;

namespace Taskist.Web.Infrastructure;

public static partial class SentryEventScrubber
{
    #region Fields

    private static readonly string[] SensitiveHeaders =
    [
        "Authorization", "Cookie", "Set-Cookie", "RequestVerificationToken", "X-XSRF-TOKEN", "X-CSRF-TOKEN",
        "X-Gitlab-Token", "X-Hub-Signature", "X-Hub-Signature-256", "X-Taskist-Signature-256", "Proxy-Authorization"
    ];

    #endregion

    #region Methods

    public static SentryEvent Scrub(SentryEvent sentryEvent)
    {
        var request = sentryEvent.Request;
        request.Cookies = null;
        request.Data = null;
        request.QueryString = null;
        request.Url = StripQueryStrings(request.Url);

        foreach (var header in request.Headers.Keys.ToList())
        {
            if (SensitiveHeaders.Any(h => string.Equals(h, header, StringComparison.OrdinalIgnoreCase)))
                request.Headers.Remove(header);
        }

        foreach (var exception in sentryEvent.SentryExceptions ?? [])
            exception.Value = StripQueryStrings(exception.Value);

        if (sentryEvent.Message != null)
        {
            sentryEvent.Message.Message = StripQueryStrings(sentryEvent.Message.Message);
            sentryEvent.Message.Formatted = StripQueryStrings(sentryEvent.Message.Formatted);
        }

        var userId = sentryEvent.User.Id;
        sentryEvent.User = new SentryUser { Id = userId };

        return sentryEvent;
    }

    public static Breadcrumb ScrubBreadcrumb(Breadcrumb breadcrumb)
    {
        var data = breadcrumb.Data?.ToDictionary(pair => pair.Key, pair => StripQueryStrings(pair.Value) ?? string.Empty);
        return new Breadcrumb(StripQueryStrings(breadcrumb.Message) ?? string.Empty, breadcrumb.Type ?? string.Empty, data, breadcrumb.Category, breadcrumb.Level);
    }

    public static string? StripQueryStrings(string? text)
    {
        return string.IsNullOrEmpty(text) ? text : QueryStringPattern().Replace(text, string.Empty);
    }

    #endregion

    #region Utilities

    [GeneratedRegex(@"(?<=(?:https?://|/)[^\s?""'<>]*)\?[^\s""'<>]*", RegexOptions.IgnoreCase)]
    private static partial Regex QueryStringPattern();

    #endregion
}
