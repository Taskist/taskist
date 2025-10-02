using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Taskist.Web.Helpers.Extensions;

namespace Taskist.Web.Helpers.Attributes;

public sealed class FormNameAttribute : TypeFilterAttribute
{
    #region Ctor

    public FormNameAttribute(string formKeyName, string actionParameterName) : base(typeof(FormNameAttributeFilter))
    {
        Arguments = [formKeyName, actionParameterName];
    }

    #endregion

    #region Nested filter

    private class FormNameAttributeFilter : IAsyncActionFilter
    {
        #region Fields

        protected readonly string _formKeyName;
        protected readonly string _actionParameterName;

        #endregion

        #region Ctor

        public FormNameAttributeFilter(string formKeyName, string actionParameterName)
        {
            _formKeyName = formKeyName;
            _actionParameterName = actionParameterName;
        }

        #endregion

        #region Utilities

        private async Task CheckFormNameAsync(ActionExecutingContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            
            context.ActionArguments[_actionParameterName] = await context.HttpContext.Request.IsFormAnyAsync(key => key.Equals(_formKeyName));
        }

        #endregion

        #region Methods

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            await CheckFormNameAsync(context);
            if (context.Result == null)
                await next();
        }

        #endregion
    }

    #endregion
}