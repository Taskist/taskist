using Sentry;
using Sentry.Protocol;
using Taskist.Web.Infrastructure;

namespace Taskist.Tests.Infrastructure;

public class SentryEventScrubberTests
{
    #region Utilities

    private static SentryEvent CreateEvent()
    {
        var sentryEvent = new SentryEvent();
        sentryEvent.Request.Url = "https://taskist.example.com/setpassword?token=secret-reset-token";
        sentryEvent.Request.QueryString = "?token=secret-reset-token";
        sentryEvent.Request.Cookies = ".AspNetCore.Cookies=session-value";
        sentryEvent.Request.Data = "{\"password\":\"hunter2\"}";
        sentryEvent.Request.Headers["Authorization"] = "Bearer glpat-secret";
        sentryEvent.Request.Headers["cookie"] = ".AspNetCore.Cookies=session-value";
        sentryEvent.Request.Headers["RequestVerificationToken"] = "antiforgery-value";
        sentryEvent.Request.Headers["X-Hub-Signature-256"] = "sha256=abc";
        sentryEvent.Request.Headers["X-Gitlab-Token"] = "webhook-secret";
        sentryEvent.Request.Headers["User-Agent"] = "Mozilla/5.0";
        sentryEvent.User = new SentryUser { Id = "42", Email = "someone@example.com", Username = "someone", IpAddress = "203.0.113.7" };
        return sentryEvent;
    }

    #endregion

    #region Tests

    [Fact]
    public void Scrub_RemovesBodyCookiesAndQueryString()
    {
        var scrubbed = SentryEventScrubber.Scrub(CreateEvent());

        Assert.Null(scrubbed.Request.Data);
        Assert.Null(scrubbed.Request.Cookies);
        Assert.Null(scrubbed.Request.QueryString);
        Assert.Equal("https://taskist.example.com/setpassword", scrubbed.Request.Url);
    }

    [Fact]
    public void Scrub_RemovesSensitiveHeaders_CaseInsensitively_AndKeepsOthers()
    {
        var headers = SentryEventScrubber.Scrub(CreateEvent()).Request.Headers;

        Assert.False(headers.ContainsKey("Authorization"));
        Assert.False(headers.ContainsKey("cookie"));
        Assert.False(headers.ContainsKey("RequestVerificationToken"));
        Assert.False(headers.ContainsKey("X-Hub-Signature-256"));
        Assert.False(headers.ContainsKey("X-Gitlab-Token"));
        Assert.Equal("Mozilla/5.0", headers["User-Agent"]);
    }

    [Theory]
    [InlineData("Request starting HTTP/1.1 POST http://localhost:5081/setpassword?token=abc123 - application/json", "Request starting HTTP/1.1 POST http://localhost:5081/setpassword - application/json")]
    [InlineData("GET /resetpassword?token=abc&email=a@b.c HTTP/1.1", "GET /resetpassword HTTP/1.1")]
    [InlineData("https://a.example/x?y=1 and https://b.example/z?q=2", "https://a.example/x and https://b.example/z")]
    [InlineData("Is this ok? Yes.", "Is this ok? Yes.")]
    [InlineData("No query here: /Backlog/Edit/38", "No query here: /Backlog/Edit/38")]
    public void StripQueryStrings_RemovesQueriesFromUrlsOnly(string input, string expected)
    {
        Assert.Equal(expected, SentryEventScrubber.StripQueryStrings(input));
    }

    [Fact]
    public void ScrubBreadcrumb_StripsQueryStringsFromMessageAndData()
    {
        var breadcrumb = new Breadcrumb("Request starting POST http://localhost/setpassword?token=secret", "default",
            new Dictionary<string, string> { ["url"] = "/setpassword?token=secret", ["eventId"] = "1" }, "Microsoft.AspNetCore.Hosting.Diagnostics");

        var scrubbed = SentryEventScrubber.ScrubBreadcrumb(breadcrumb);

        Assert.DoesNotContain("secret", scrubbed.Message);
        Assert.Equal("/setpassword", scrubbed.Data!["url"]);
        Assert.Equal("1", scrubbed.Data!["eventId"]);
        Assert.Equal("Microsoft.AspNetCore.Hosting.Diagnostics", scrubbed.Category);
    }

    [Fact]
    public void Scrub_StripsQueryStringsFromExceptionMessages()
    {
        var sentryEvent = CreateEvent();
        sentryEvent.SentryExceptions = [new SentryException { Type = "HttpRequestException", Value = "Failed calling https://api.example/hook?secret=abc" }];

        var scrubbed = SentryEventScrubber.Scrub(sentryEvent);

        Assert.Equal("Failed calling https://api.example/hook", scrubbed.SentryExceptions!.Single().Value);
    }

    [Fact]
    public void Scrub_KeepsOnlyTheUserId()
    {
        var user = SentryEventScrubber.Scrub(CreateEvent()).User;

        Assert.Equal("42", user.Id);
        Assert.Null(user.Email);
        Assert.Null(user.Username);
        Assert.Null(user.IpAddress);
    }

    #endregion
}
