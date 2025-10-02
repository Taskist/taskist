using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Net;

namespace Taskist.Core.Common;

public class HttpHelper : IHttpHelper
{
    #region Fields

    private readonly IHttpContextAccessor _httpContextAccessor;

    #endregion

    #region Ctor

    public HttpHelper(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    #endregion

    #region Method

    public string GetBaseURL()
    {
        var request = _httpContextAccessor.HttpContext.Request;
        return $"{request.Scheme}://{request.Host.Value}{request.PathBase.Value}";
    }

    public string GetCurrentIpAddress()
    {
        if (_httpContextAccessor?.HttpContext == null || _httpContextAccessor.HttpContext.Request == null)
            return string.Empty;

        var result = string.Empty;
        try
        {
            if (_httpContextAccessor.HttpContext.Request.Headers != null)
            {
                var forwardedHttpHeaderKey = "X-FORWARDED-FOR";
                var forwardedHeader = _httpContextAccessor.HttpContext.Request.Headers[forwardedHttpHeaderKey];
                if (!StringValues.IsNullOrEmpty(forwardedHeader))
                    result = forwardedHeader.FirstOrDefault();
            }

            if (string.IsNullOrEmpty(result) && _httpContextAccessor.HttpContext.Connection.RemoteIpAddress != null)
                result = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
        }
        catch
        {
            return string.Empty;
        }

        if (result != null && result.Equals(IPAddress.IPv6Loopback.ToString(), StringComparison.InvariantCultureIgnoreCase))
            result = IPAddress.Loopback.ToString();

        if (IPAddress.TryParse(result ?? string.Empty, out var ip))
            result = ip.ToString();
        else if (!string.IsNullOrEmpty(result))
            result = result.Split(':').FirstOrDefault();

        return result;
    }

    public bool IsCurrentConnectionSecured()
    {
        if (!IsRequestAvailable())
            return false;

        return _httpContextAccessor.HttpContext.Request.IsHttps;
    }

    public string GetUrlReferrer()
    {
        if (!IsRequestAvailable())
            return string.Empty;

        //URL referrer is null in some case (for example, in IE 8)
        return _httpContextAccessor.HttpContext.Request.Headers[HeaderNames.Referer];
    }

    public string GetCurrentPageUrl(bool includeQueryString, bool lowercaseUrl = false)
    {
        if (!IsRequestAvailable())
            return string.Empty;

        var pageUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}{_httpContextAccessor.HttpContext.Request.Path}";

        if (includeQueryString)
            pageUrl = $"{pageUrl}{_httpContextAccessor.HttpContext.Request.QueryString}";

        if (lowercaseUrl)
            pageUrl = pageUrl.ToLowerInvariant();

        return pageUrl;
    }

    public bool IsRequestBeingRedirected
    {
        get
        {
            var response = _httpContextAccessor.HttpContext.Response;
            int[] redirectionStatusCodes = { StatusCodes.Status301MovedPermanently, StatusCodes.Status302Found };

            return redirectionStatusCodes.Contains(response.StatusCode);
        }
    }

    public bool IsPostBeingDone
    {
        get
        {
            if (_httpContextAccessor.HttpContext.Items[Constant.IsPostBeingDoneRequestItem] == null)
                return false;

            return Convert.ToBoolean(_httpContextAccessor.HttpContext.Items[Constant.IsPostBeingDoneRequestItem]);
        }

        set => _httpContextAccessor.HttpContext.Items[Constant.IsPostBeingDoneRequestItem] = value;
    }

    public string GetCurrentRequestProtocol()
    {
        return IsCurrentConnectionSecured() ? Uri.UriSchemeHttps : Uri.UriSchemeHttp;
    }

    public bool IsLocalRequest(HttpRequest req)
    {
        var connection = req.HttpContext.Connection;
        if (IsIpAddressSet(connection.RemoteIpAddress))
        {
            return IsIpAddressSet(connection.LocalIpAddress)
                ? connection.RemoteIpAddress.Equals(connection.LocalIpAddress)
                : IPAddress.IsLoopback(connection.RemoteIpAddress);
        }

        return true;
    }

    public bool IsStaticResource()
    {
        if (!IsRequestAvailable())
            return false;

        string path = _httpContextAccessor.HttpContext.Request.Path;
        var contentTypeProvider = new FileExtensionContentTypeProvider();
        return contentTypeProvider.TryGetContentType(path, out var _);
    }

    public string GetRawUrl(HttpRequest request)
    {
        var rawUrl = request.HttpContext.Features.Get<IHttpRequestFeature>()?.RawTarget;

        if (string.IsNullOrEmpty(rawUrl))
            rawUrl = $"{request.PathBase}{request.Path}{request.QueryString}";

        return rawUrl;
    }

    public bool IsAjaxRequest(HttpRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (request.Headers == null)
            return false;

        return request.Headers["X-Requested-With"] == "XMLHttpRequest";
    }

    protected bool IsRequestAvailable()
    {
        if (_httpContextAccessor?.HttpContext == null)
            return false;

        try
        {
            if (_httpContextAccessor.HttpContext.Request == null)
                return false;
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    protected bool IsIpAddressSet(IPAddress address)
    {
        var rez = address != null && address.ToString() != IPAddress.IPv6Loopback.ToString();

        return rez;
    }

    #endregion
}
