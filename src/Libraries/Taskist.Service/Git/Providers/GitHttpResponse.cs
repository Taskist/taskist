using System.Net.Http.Headers;

namespace Taskist.Service.Git.Providers;

public class GitHttpResponse
{
    public int StatusCode { get; init; }

    public bool Success => StatusCode is >= 200 and < 300;

    public string? Content { get; init; }

    public HttpResponseHeaders? Headers { get; init; }

    public string? TransportError { get; init; }

    public string? Header(string name)
    {
        return Headers != null && Headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;
    }
}
