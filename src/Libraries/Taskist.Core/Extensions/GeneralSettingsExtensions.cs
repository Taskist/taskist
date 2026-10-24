using Taskist.Core.Domain.Settings;

namespace Taskist.Core.Extensions;

public static class GeneralSettingsExtensions
{
    #region Fields

    public const string DefaultCompanyName = "Taskist";

    #endregion

    #region Methods

    public static string CompanyNameOrDefault(this GeneralSettings? settings)
    {
        return string.IsNullOrWhiteSpace(settings?.CompanyName) ? DefaultCompanyName : settings.CompanyName.Trim();
    }

    #endregion
}
