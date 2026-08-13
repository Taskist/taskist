using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Taskist.Core.Common;
using Taskist.Service.Logging;

namespace Taskist.Web.Helpers.Common;

public class GlobalExceptionHandler : IExceptionHandler
{
    #region Fields

    protected readonly IServiceScopeFactory _scopeFactory;

    #endregion

    #region Ctor

    /// <summary>
    /// The handler is registered as a singleton, so the scoped services it needs are
    /// resolved from a per-request scope rather than captured in the constructor.
    /// </summary>
    public GlobalExceptionHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    #endregion

    #region Methods

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        using var scope = _scopeFactory.CreateScope();

        var workContext = scope.ServiceProvider.GetRequiredService<IWorkContext>();
        var httpHelper = scope.ServiceProvider.GetRequiredService<IHttpHelper>();
        var logService = scope.ServiceProvider.GetRequiredService<ILogService>();

        int id = await logService.ErrorAndGetIdAsync(exception.Message, exception, await workContext.GetCurrentUserAsync());

        if (httpHelper.IsAjaxRequest(httpContext.Request))
        {
            await httpContext.Response.WriteAsJsonAsync(exception.Message, cancellationToken);
        }
        else
        {
            httpContext.Response.Redirect($"{httpHelper.GetBaseURL()}/Error/{id}");
        }

        return true;
    }

    #endregion
}