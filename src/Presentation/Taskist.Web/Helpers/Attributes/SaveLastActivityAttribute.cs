using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Taskist.Core.Common;
using Taskist.Core.Domain.Users;
using Taskist.Data.Repository;
using Taskist.Web.Helpers.Extensions;

namespace Taskist.Web.Helpers.Attributes;

public sealed class SaveLastActivityAttribute : TypeFilterAttribute
{
    #region Ctor

    public SaveLastActivityAttribute() : base(typeof(SaveLastActivityFilter))
    {
    }

    #endregion

    #region Nested filter

    private class SaveLastActivityFilter : IAsyncActionFilter
    {
        #region Fields

        protected readonly IRepository<User> _userRepository;
        protected readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public SaveLastActivityFilter(IRepository<User> userRepository, IWorkContext workContext)
        {
            _userRepository = userRepository;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

        private async Task SaveLastActivityAsync(ActionExecutingContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (!context.HttpContext.Request.IsGetRequest())
                return;

            var user = await _workContext.GetCurrentUserAsync();
            if (user.LastActivityDate.AddMinutes(15) < DateTime.UtcNow)
            {
                user.LastActivityDate = DateTime.UtcNow;
                await _userRepository.UpdateAsync(user);
            }
        }

        #endregion

        #region Methods

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await SaveLastActivityAsync(context);
            if (context.Result == null)
                await next();
        }

        #endregion
    }

    #endregion
}