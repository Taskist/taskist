using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Taskist.Core.Domain.Users;
using Taskist.Service.Common;
using Taskist.Web.Infrastructure;

namespace Taskist.Web.Helpers.TwoFactor;

public class TwoFactorSetupRequiredFilter : IAsyncActionFilter
{
    #region Fields

    private readonly TwoFactorOptions _options;

    #endregion

    #region Ctor

    public TwoFactorSetupRequiredFilter(TwoFactorOptions options)
    {
        _options = options;
    }

    #endregion

    #region Methods

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;
        if (!_options.RequireForAdministrators || user?.Identity?.IsAuthenticated != true)
        {
            await next();
            return;
        }

        var isAdministrator = user.IsInRole(UserConstant.SystemAdministratorRoleName) || user.IsInRole(UserConstant.AdministratorsRoleName);
        var method = user.FindFirst(ServiceConstant.AuthMethodClaim)?.Value;
        var controller = (context.ActionDescriptor as ControllerActionDescriptor)?.ControllerName;

        if (!TwoFactorSetupPolicy.IsSetupRequired(true, isAdministrator, method) || TwoFactorSetupPolicy.IsAllowedDuringSetup(controller))
        {
            await next();
            return;
        }

        var request = context.HttpContext.Request;
        var isAjax = string.Equals(request.Headers.XRequestedWith, "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);

        context.Result = isAjax || !HttpMethods.IsGet(request.Method)
            ? new ContentResult
            {
                StatusCode = StatusCodes.Status403Forbidden,
                ContentType = "text/plain; charset=utf-8",
                Content = "Set up two-factor sign-in under My profile → Security first."
            }
            : new RedirectResult($"{request.PathBase}/Profile?setup2fa=1#security");
    }

    #endregion
}
