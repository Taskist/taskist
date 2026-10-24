using Taskist.Core.Domain.Settings;
using Taskist.Core.Extensions;

namespace Taskist.Tests.Extensions;

public class GeneralSettingsExtensionsTests
{
    #region Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CompanyNameOrDefault_FallsBackToTaskist_WhenNotSet(string? companyName)
    {
        var settings = new GeneralSettings { CompanyName = companyName };

        Assert.Equal(GeneralSettingsExtensions.DefaultCompanyName, settings.CompanyNameOrDefault());
    }

    [Fact]
    public void CompanyNameOrDefault_FallsBackToTaskist_WhenSettingsMissing()
    {
        GeneralSettings? settings = null;

        Assert.Equal(GeneralSettingsExtensions.DefaultCompanyName, settings.CompanyNameOrDefault());
    }

    [Fact]
    public void CompanyNameOrDefault_ReturnsTrimmedConfiguredName()
    {
        var settings = new GeneralSettings { CompanyName = "  Acme Corporation " };

        Assert.Equal("Acme Corporation", settings.CompanyNameOrDefault());
    }

    #endregion
}
