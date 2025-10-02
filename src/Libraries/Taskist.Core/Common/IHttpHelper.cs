using Microsoft.AspNetCore.Http;

namespace Taskist.Core.Common;

public interface IHttpHelper
{
    string GetBaseURL();

    string GetUrlReferrer();

    string GetCurrentPageUrl(bool includeQueryString, bool lowercaseUrl = false);

    string GetCurrentIpAddress();

    bool IsCurrentConnectionSecured();

    bool IsRequestBeingRedirected { get; }

    bool IsPostBeingDone { get; set; }

    string GetCurrentRequestProtocol();

    bool IsLocalRequest(HttpRequest req);

    bool IsStaticResource();

    string GetRawUrl(HttpRequest request);

    bool IsAjaxRequest(HttpRequest request);
}
