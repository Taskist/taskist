using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Taskist.Core.Dtos.Git;

namespace Taskist.Service.Git.Providers;

public abstract class GitProviderBase
{
    #region Fields

    protected readonly GitProviderContext Context;

    #endregion

    #region Ctor

    protected GitProviderBase(GitProviderContext context)
    {
        Context = context;
    }

    #endregion

    #region Properties

    public int ConnectionId => Context.Connection.Id;

    public string? LastError { get; protected set; }

    protected string? ServiceTokenError { get; set; }

    protected abstract string ProviderName { get; }

    #endregion

    #region Http

    protected async Task<GitHttpResponse> SendRawAsync(HttpMethod method, string pathOrUrl, string? token, object? body = null,
        Action<HttpRequestMessage>? configure = null, bool absolute = false)
    {
        try
        {
            using var request = new HttpRequestMessage(method, absolute ? new Uri(pathOrUrl) : new Uri(Context.ApiBase, pathOrUrl));
            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.UserAgent.ParseAdd("Taskist-Git-Integration/1.0");
            request.Headers.Accept.ParseAdd("application/json");
            ConfigureRequest(request);
            configure?.Invoke(request);

            if (body is HttpContent content)
                request.Content = content;
            else if (body != null)
                request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            using var response = await Context.Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            var text = await ReadLimitedAsync(response.Content);

            return new GitHttpResponse
            {
                StatusCode = (int)response.StatusCode,
                Content = text,
                Headers = response.Headers
            };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException or InvalidOperationException)
        {
            return new GitHttpResponse { StatusCode = 0, TransportError = ex.Message };
        }
    }

    protected async Task<GitResult<T>> SendAsync<T>(HttpMethod method, string path, string? token, object? body = null,
        bool personal = false, Action<HttpRequestMessage>? configure = null) where T : class
    {
        if (string.IsNullOrWhiteSpace(token))
            return Remember(GitResult<T>.Fail(personal
                ? $"Connect your {ProviderName} account (My profile → Git connections) to do this."
                : ServiceTokenError ?? $"{ProviderName} is not configured. An administrator can set it up under Integrations → Git Connections."));

        var response = await SendRawAsync(method, path, token, body, configure);
        if (response.StatusCode == 0)
            return Remember(GitResult<T>.Fail($"Could not reach {ProviderName}: {response.TransportError ?? "no response"}"));

        if (!response.Success)
            return Remember(GitResult<T>.Fail(DescribeError(response.StatusCode, response.Content, personal), response.StatusCode));

        LastError = null;
        return GitResult<T>.Ok(Deserialize<T>(response.Content)!, response.StatusCode);
    }

    protected async Task<T?> GetAsync<T>(string path, string? token = null) where T : class
    {
        var result = await SendAsync<T>(HttpMethod.Get, path, token ?? await ServiceTokenAsync(path));
        return result.Success ? result.Data : null;
    }

    protected abstract Task<string?> ServiceTokenAsync(string? path);

    protected virtual void ConfigureRequest(HttpRequestMessage request)
    {
    }

    protected static T? Deserialize<T>(string? content) where T : class
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        if (typeof(T) == typeof(object))
            return JToken.Parse(content) as T;

        return JsonConvert.DeserializeObject<T>(content);
    }

    protected GitResult<T> Remember<T>(GitResult<T> result)
    {
        LastError = result.Error;
        return result;
    }

    protected virtual string DescribeError(int status, string? content, bool personal)
    {
        var message = ExtractMessage(content);
        var prefix = status switch
        {
            401 => personal
                ? $"{ProviderName} rejected your token (401). It may be expired or revoked. Reconnect under My profile → Git connections."
                : $"{ProviderName} rejected the Taskist service credentials (401). An administrator can replace them under Configuration → Git connections.",
            403 => personal
                ? $"{ProviderName} does not allow you to do this (403). Check your role in the repository and the token's permissions."
                : $"The Taskist service credentials are not allowed to do this (403). Check the token's scopes or the app's permissions.",
            404 => $"Not found in {ProviderName} (404). Check the repository link and that the credentials can see it.",
            405 => $"{ProviderName} cannot do this right now (405). It may be a draft, have conflicts, or be blocked by a rule.",
            406 => $"{ProviderName} cannot merge this (406). It may have conflicts or need a rebase.",
            409 => $"It changed since it was loaded (409). Refresh and try again.",
            422 => $"{ProviderName} rejected the request (422).",
            429 => $"{ProviderName} rate limit reached (429). Try again in a minute.",
            _ => $"{ProviderName} returned an error ({status})."
        };

        return string.IsNullOrWhiteSpace(message) ? prefix : $"{prefix} {ProviderName} says: {message}";
    }

    protected static string? ExtractMessage(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            var token = JToken.Parse(content);
            if (token is not JObject obj)
                return null;

            var message = obj["message"] ?? obj["error_description"] ?? obj["error"];
            if (message == null)
                return null;

            var text = message.Type == JTokenType.String ? message.ToString() : message.ToString(Formatting.None);
            var errors = obj["errors"];
            if (errors is JArray array && array.Count > 0)
                text += " " + string.Join("; ", array.Select(e => e["message"]?.ToString() ?? e.ToString(Formatting.None)));

            return Truncate(text, 400);
        }
        catch (JsonException)
        {
            return Truncate(content, 300);
        }
    }

    protected static string Truncate(string value, int length) => value.Length <= length ? value : value[..length];

    protected static string Escape(string? value) => Uri.EscapeDataString(value?.Trim() ?? string.Empty);

    private static async Task<string?> ReadLimitedAsync(HttpContent content)
    {
        await using var stream = await content.ReadAsStreamAsync();
        using var buffer = new MemoryStream();
        var chunk = new byte[81920];
        int read;
        while ((read = await stream.ReadAsync(chunk)) > 0)
        {
            if (buffer.Length + read > GitHttp.MaxResponseBytes)
                throw new IOException("The response from the Git server was too large.");
            buffer.Write(chunk, 0, read);
        }

        return buffer.Length == 0 ? null : Encoding.UTF8.GetString(buffer.ToArray());
    }

    #endregion
}
