using Taskist.Core.Common;
using Taskist.Core.Configuration;

namespace Taskist.Service.Messages;

public class AppLinkBuilder : IAppLinkBuilder
{
    private readonly IAppUrlProvider _appUrlProvider;
    private readonly IHttpHelper _httpHelper;

    public AppLinkBuilder(IAppUrlProvider appUrlProvider, IHttpHelper httpHelper)
    {
        _appUrlProvider = appUrlProvider;
        _httpHelper = httpHelper;
    }

    public string Build(string relativePath)
    {
        var baseUrl = _appUrlProvider.BaseUrl;
        if (string.IsNullOrWhiteSpace(baseUrl))
            baseUrl = ResolveFromRequest();

        baseUrl = (baseUrl ?? string.Empty).TrimEnd('/');

        if (string.IsNullOrEmpty(relativePath))
            return baseUrl;

        return $"{baseUrl}/{relativePath.TrimStart('/')}";
    }

    private string ResolveFromRequest()
    {
        try
        {
            return _httpHelper.GetBaseURL();
        }
        catch
        {
            return string.Empty;
        }
    }
}
