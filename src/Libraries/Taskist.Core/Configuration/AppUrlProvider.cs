namespace Taskist.Core.Configuration;

public class AppUrlProvider : IAppUrlProvider
{
    public AppUrlProvider(string baseUrl)
    {
        BaseUrl = baseUrl ?? string.Empty;
    }

    public string BaseUrl { get; }
}
