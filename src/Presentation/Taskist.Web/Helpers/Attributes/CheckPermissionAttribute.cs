using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using Taskist.Core.Common;
using Taskist.Service.Localization;
using Taskist.Service.Security;

namespace Taskist.Web.Helpers.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class CheckPermissionAttribute : TypeFilterAttribute
{
    #region Ctor

    public CheckPermissionAttribute(string permissionSystemName,
        CheckPermissionResultType resultType = CheckPermissionResultType.Default)
        : base(typeof(CheckPermissionFilter))
    {
        Arguments = [resultType, new List<string> { permissionSystemName }];
        PermissionSystemName.Add(permissionSystemName);
        ResultType = resultType;
    }

    public CheckPermissionAttribute(string[] permissionSystemNames,
        CheckPermissionResultType resultType = CheckPermissionResultType.Default)
        : base(typeof(CheckPermissionFilter))
    {
        Arguments = [resultType, permissionSystemNames.ToList()];
        PermissionSystemName.AddRange(permissionSystemNames);
        ResultType = resultType;
    }

    #endregion

    #region Properties

    public List<string> PermissionSystemName { get; } = new();

    public CheckPermissionResultType ResultType { get; }

    #endregion

    #region Nested filter

    private class CheckPermissionFilter : IAsyncAuthorizationFilter
    {

        #region Fields

        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly ILocalizationService _localizationService;
        protected readonly IPermissionService _permissionService;
        protected readonly IHttpHelper _httpHelper;
        protected readonly CheckPermissionResultType _resultType;
        protected readonly List<string> _permissionSystemNames;

        #endregion

        #region Ctor

        public CheckPermissionFilter(CheckPermissionResultType resultType,
            List<string> permissionSystemNames,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            IHttpHelper httpHelper)
        {
            _resultType = resultType;
            _permissionSystemNames = permissionSystemNames;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _httpHelper = httpHelper;
        }

        #endregion

        #region Utilities

        private async Task AuthorizeAsync(AuthorizationFilterContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (!context.Filters.Any(filter => filter is CheckPermissionFilter))
                return;

            foreach (var permissionSystemName in _permissionSystemNames)
                if (await _permissionService.AuthorizeAsync(permissionSystemName))
                    return;

            var resultType = _resultType;

            var request = _httpContextAccessor.HttpContext?.Request;

            if (request == null)
                return;

            if (resultType == CheckPermissionResultType.Default)
                resultType = request.Method switch
                {
                    WebRequestMethods.Http.Post => _httpHelper.IsAjaxRequest(request) ? CheckPermissionResultType.Json : CheckPermissionResultType.Html,
                    WebRequestMethods.Http.Get => CheckPermissionResultType.Html,
                    _ => CheckPermissionResultType.Text,
                };

            IActionResult html()
            {
                return new RedirectToActionResult("AccessDenied", "Common", null);
            }

            context.Result = resultType switch
            {
                CheckPermissionResultType.Json => new JsonResult(new
                {
                    error = await _localizationService.GetResourceAsync("Admin.AccessDenied.Description")
                }),
                CheckPermissionResultType.Html => html(),
                CheckPermissionResultType.Text => new ContentResult
                {
                    Content = await _localizationService.GetResourceAsync("Admin.AccessDenied.Description"),
                    ContentType = "text/plain",
                },
                _ => context.Result
            };
        }

        #endregion

        #region Methods

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            await AuthorizeAsync(context);
        }

        #endregion
    }

    #endregion

    #region Nested class

    public enum CheckPermissionResultType
    {
        Default = 0,
        Html = 1,
        Text = 2,
        Json = 3,
        NoData = 4
    }

    #endregion
}