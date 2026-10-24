using Taskist.Core.Common;
using Taskist.Service.Logging;
using Microsoft.AspNetCore.Diagnostics;

namespace Taskist.Web.Helpers.Common;

public class GlobalExceptionHandler : IExceptionHandler
{
    #region Methods

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {

        var services = httpContext.RequestServices;
        var workContext = services.GetRequiredService<IWorkContext>();
        var httpHelper = services.GetRequiredService<IHttpHelper>();
        var logService = services.GetRequiredService<ILogService>();

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
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
