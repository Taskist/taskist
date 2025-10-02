using Taskist.Core.Common;
using Taskist.Service.Logging;
using Microsoft.AspNetCore.Diagnostics;

namespace Taskist.Web.Helpers.Common;

public class GlobalExceptionHandler : IExceptionHandler
{
    #region Fields

    protected readonly IWorkContext _workContext;
    protected readonly IHttpHelper _httpHelper;
    protected readonly ILogService _logService;

    #endregion

    #region Ctor

    public GlobalExceptionHandler(IWorkContext workContext, IHttpHelper httpHelper, ILogService logService)
    {
        _workContext = workContext;
        _httpHelper = httpHelper;
        _logService = logService;
    }

    #endregion

    #region Methods

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        int id = await _logService.ErrorAndGetIdAsync(exception.Message, exception, await _workContext.GetCurrentUserAsync());

        if (_httpHelper.IsAjaxRequest(httpContext.Request))
        {
            await httpContext.Response.WriteAsJsonAsync(exception.Message, cancellationToken);
        }
        else
        {
            httpContext.Response.Redirect($"{_httpHelper.GetBaseURL()}/Error/{id}");
        }

        return true;
    }

    #endregion
}