using Taskist.Web.Helpers.Sso;

namespace Taskist.Tests.Sso;

public class LocalLoginSwitchTests
{
    #region Tests

    [Theory]
    [InlineData("1", true)]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData(" TRUE ", true)]
    [InlineData("0", false)]
    [InlineData("false", false)]
    [InlineData("yes", false)]
    [InlineData("11", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsOn_AcceptsOneOrTrue(string? value, bool expected)
    {
        Assert.Equal(expected, LocalLoginSwitch.IsOn(value));
    }

    #endregion
}
