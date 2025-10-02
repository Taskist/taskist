using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Taskist.Core.Common;
using Taskist.Core.Domain.Users;
using Taskist.Data.Repository;
using Taskist.Web.Helpers.Extensions;

namespace Taskist.Web.Helpers.Attributes;

public sealed class SaveIpAddressAttribute : TypeFilterAttribute
{
    #region Ctor

    public SaveIpAddressAttribute() : base(typeof(SaveIpAddressFilter))
    {
    }

    #endregion

    #region Nested filter

    private class SaveIpAddressFilter : IAsyncActionFilter
    {
        #region Fields

        protected readonly IRepository<User> _userRepository;
        protected readonly IHttpHelper _httpHelper;
        protected readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public SaveIpAddressFilter(IRepository<User> userRepository, IHttpHelper httpHelper,
            IWorkContext workContext)
        {
            _userRepository = userRepository;
            _httpHelper = httpHelper;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

        private async Task SaveIpAddressAsync(ActionExecutingContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (!context.HttpContext.Request.IsGetRequest())
                return;

            var currentIpAddress = _httpHelper.GetCurrentIpAddress();

            if (string.IsNullOrEmpty(currentIpAddress))
                return;

            var user = await _workContext.GetCurrentUserAsync();
            if (!currentIpAddress.Equals(user.LastIPAddress, StringComparison.InvariantCultureIgnoreCase))
            {
                user.LastIPAddress = currentIpAddress;
                await _userRepository.UpdateAsync(user);
            }
        }

        #endregion

        #region Methods

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await SaveIpAddressAsync(context);
            if (context.Result == null)
                await next();
        }

        #endregion
    }

    #endregion
}