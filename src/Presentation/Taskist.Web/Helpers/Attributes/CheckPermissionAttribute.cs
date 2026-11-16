using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Taskist.Core.Common;
using Taskist.Service.Logging;
using Taskist.Service.Security;

namespace Taskist.Web.Helpers.Attributes;

public class CheckPermissionAttribute : TypeFilterAttribute
{
    #region Ctor

    public CheckPermissionAttribute(string permissionSystemName, DenyMode denyMode = DenyMode.Auto)
            : base(typeof(CheckPermissionFilter))
    {
        Arguments = [permissionSystemName, denyMode];
    }

    #endregion

    #region Nested filter

    private class CheckPermissionFilter : IAsyncAuthorizationFilter
    {
        #region Fields

        protected readonly string _permissionSystemName;
        protected readonly DenyMode _denyMode;
        protected readonly IPermissionService _permissionService;
        protected readonly IUserActivityService _activityService;

        #endregion

        #region Ctor

        public CheckPermissionFilter(string permissionSystemName, DenyMode denyMode,
                    IPermissionService permissionService,
                    IUserActivityService activityService)
        {
            _permissionSystemName = permissionSystemName;
            _denyMode = denyMode;
            _permissionService = permissionService;
            _activityService = activityService;
        }

        #endregion

        #region Utilities

        private async Task<DenyMode> ResolveModeAsync(HttpRequest request)
        {
            if (_denyMode != DenyMode.Auto)
                return _denyMode;

            var hasDraw = request.Query.ContainsKey("draw");
            if (!hasDraw && request.HasFormContentType)
            {
                var form = await request.ReadFormAsync();
                hasDraw = form.ContainsKey("draw");
            }

            if (hasDraw)
                return DenyMode.DataRead;

            if (string.Equals(request.Headers["X-Requested-With"], "XMLHttpRequest",
                            StringComparison.OrdinalIgnoreCase))
                return DenyMode.Partial;

            return DenyMode.Page;
        }

        #endregion

        #region Methods

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (await _permissionService.AuthorizeAsync(_permissionSystemName))
                return;

            await _activityService.InsertAsync(ActivityType.AccessDenied,
                $"Denied {_permissionSystemName} at {context.HttpContext.Request.Path}");

            var mode = await ResolveModeAsync(context.HttpContext.Request);

            context.Result = mode switch
            {
                DenyMode.DataRead => new JsonResult(new { error = "Access denied!" }),
                DenyMode.Partial => new RedirectToActionResult("AccessDeniedPartial", "Common", null),
                _ => new RedirectToActionResult("AccessDenied", "Common", null)
            };
        }

        #endregion
    }

    #endregion
}
