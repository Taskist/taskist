using Taskist.Core.Common;
using Taskist.Core.Domain.Settings;
using Taskist.Service.Masters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Taskist.Web.Helpers.Attributes;

public class ValidateIpAddressAttribute : TypeFilterAttribute
{
    #region Ctor

    public ValidateIpAddressAttribute() : base(typeof(ValidateIpAddressFilter))
    {
    }

    #endregion

    #region Nested filter

    private class ValidateIpAddressFilter : IAsyncActionFilter
    {
        #region Fields

        protected readonly CommonSettings _commonSettings;
        protected readonly IHttpHelper _httpHelper;
        protected readonly IWorkContext _workContext;
        protected readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public ValidateIpAddressFilter(CommonSettings commonSettings,
            IHttpHelper httpHelper,
            IWorkContext workContext,
            ISettingService settingService)
        {
            _commonSettings = commonSettings;
            _httpHelper = httpHelper;
            _workContext = workContext;
            _settingService = settingService;
        }

        #endregion

        #region Utilities

        private async Task ValidateIpAddress(ActionExecutingContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.HttpContext.Request == null)
                return;

            var actionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            var actionName = actionDescriptor?.ActionName;
            var controllerName = actionDescriptor?.ControllerName;

            if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(controllerName))
                return;

            if ((controllerName.Equals("Security", StringComparison.InvariantCultureIgnoreCase) &&
                actionName.Equals("AccessDenied", StringComparison.InvariantCultureIgnoreCase)) ||
                (controllerName.Equals("Security", StringComparison.InvariantCultureIgnoreCase) &&
                actionName.Equals("NoPrivilege", StringComparison.InvariantCultureIgnoreCase)))
            {
                return;
            }

            var ipAddresses = _commonSettings.AllowedIpAddress?.Split(new char[','], StringSplitOptions.RemoveEmptyEntries);

            if (ipAddresses == null || !ipAddresses.Any())
                return;

            var currentIp = _httpHelper.GetCurrentIpAddress();

            if (ipAddresses.Any(ip => ip.Equals(currentIp, StringComparison.InvariantCultureIgnoreCase)))
                return;

            context.Result = new RedirectToActionResult("AccessDenied", "Security", context.RouteData.Values);
        }

        #endregion

        #region Methods

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await ValidateIpAddress(context);
            if (context.Result == null)
                await next();
        }

        #endregion
    }

    #endregion
}