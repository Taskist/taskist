using Taskist.Web.Helpers.TwoFactor;
using Taskist.Web.Infrastructure;

namespace Taskist.Tests.Security;

public class TwoFactorSetupPolicyTests
{
    #region Tests

    [Theory]
    [InlineData(true, true, "pwd", true)]
    [InlineData(true, true, "pwd+otp", false)]
    [InlineData(true, true, "pwd+device", false)]
    [InlineData(true, true, "sso", false)]
    [InlineData(true, false, "pwd", false)]
    [InlineData(false, true, "pwd", false)]
    [InlineData(true, true, null, false)]
    public void SetupPolicy_RequiresEnrolment_OnlyForAdminsSignedInWithPasswordAlone(bool required, bool admin, string? method, bool expected)
    {
        Assert.Equal(expected, TwoFactorSetupPolicy.IsSetupRequired(required, admin, method));
    }

    [Theory]
    [InlineData("Profile", true)]
    [InlineData("account", true)]
    [InlineData("Common", true)]
    [InlineData("Backlog", false)]
    [InlineData("User", false)]
    [InlineData(null, false)]
    public void SetupPolicy_AllowsOnlyTheEnrolmentPages(string? controller, bool expected)
    {
        Assert.Equal(expected, TwoFactorSetupPolicy.IsAllowedDuringSetup(controller));
    }

    [Theory]
    [InlineData(30, 30)]
    [InlineData(0, 0)]
    [InlineData(-5, 0)]
    [InlineData(5000, TwoFactorOptions.MaxRememberDeviceDays)]
    public void RememberDeviceDays_IsClamped(int configured, int expected)
    {
        var options = new TwoFactorOptions { RememberDeviceDays = configured };

        Assert.Equal(expected, options.EffectiveRememberDeviceDays);
        Assert.Equal(expected > 0, options.CanRememberDevice);
    }

    #endregion
}
