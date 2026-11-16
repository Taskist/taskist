using Microsoft.AspNetCore.DataProtection;
using Taskist.Web.Helpers.TwoFactor;

namespace Taskist.Tests.Security;

public class RememberedDeviceTokenTests
{
    #region Fields

    private static readonly IDataProtectionProvider Provider = new EphemeralDataProtectionProvider();

    private static readonly IDataProtector Protector = Provider.CreateProtector(RememberedDeviceToken.Purpose);

    private static readonly DateTimeOffset Now = new(2026, 11, 13, 10, 0, 0, TimeSpan.Zero);

    #endregion

    #region Tests

    [Fact]
    public void ValidToken_IsAccepted()
    {
        var token = RememberedDeviceToken.Create(Protector, 7, 1234, Now.AddDays(30));

        Assert.True(RememberedDeviceToken.IsValid(Protector, token, 7, 1234, Now));
    }

    [Fact]
    public void Token_ForAnotherUser_IsRejected()
    {
        var token = RememberedDeviceToken.Create(Protector, 7, 1234, Now.AddDays(30));

        Assert.False(RememberedDeviceToken.IsValid(Protector, token, 8, 1234, Now));
    }

    [Fact]
    public void Token_IsRejected_AfterTwoFactorWasResetOrReEnabled()
    {
        var token = RememberedDeviceToken.Create(Protector, 7, 1234, Now.AddDays(30));

        Assert.False(RememberedDeviceToken.IsValid(Protector, token, 7, 5678, Now));
    }

    [Fact]
    public void ExpiredToken_IsRejected()
    {
        var token = RememberedDeviceToken.Create(Protector, 7, 1234, Now.AddMinutes(-1));

        Assert.False(RememberedDeviceToken.IsValid(Protector, token, 7, 1234, Now));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("not-a-protected-value")]
    public void MissingOrForgedToken_IsRejected(string? token)
    {
        Assert.False(RememberedDeviceToken.IsValid(Protector, token, 7, 1234, Now));
    }

    [Fact]
    public void Token_FromAnotherPurpose_IsRejected()
    {
        var other = Provider.CreateProtector("Taskist.Something.Else");
        var token = other.Protect("7|1234|" + Now.AddDays(30).ToUnixTimeSeconds());

        Assert.False(RememberedDeviceToken.IsValid(Protector, token, 7, 1234, Now));
    }

    #endregion
}
