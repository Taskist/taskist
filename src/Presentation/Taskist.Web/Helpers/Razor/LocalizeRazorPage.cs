using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Nop.Web.Framework.Localization;
using Taskist.Web.Helpers.Localization;
using Taskist.Core.Common;
using Taskist.Service.Localization;

namespace Taskist.Web.Helpers.Razor;

public abstract class LocalizeRazorPage<TModel> : RazorPage<TModel>
{
    #region Fields

    [RazorInject]
    protected IWorkContext WorkContext { get; set; }

    [RazorInject]
    protected ILocalizationService LocalizationService { get; set; }

    #endregion        

    private Localizer _localizer;

    public Localizer Localize
    {
        get
        {
            if (_localizer == null)
            {
                var language = WorkContext.GetCurrentUserLanguageAsync().Result;

                if (language != null)
                {
                    _localizer = (resourceKey, args) =>
                    {
                        var stringResource = LocalizationService.GetResourceAsync(language.Id, resourceKey).Result;

                        if (stringResource == null || string.IsNullOrEmpty(stringResource))
                        {
                            return new LocalizedString(resourceKey);
                        }

                        return new LocalizedString(args == null || args.Length == 0
                            ? stringResource
                            : string.Format(stringResource, args));
                    };
                }
            }
            return _localizer;
        }
    }
}

public abstract class LocalizeRazorPage : LocalizeRazorPage<dynamic>
{ }