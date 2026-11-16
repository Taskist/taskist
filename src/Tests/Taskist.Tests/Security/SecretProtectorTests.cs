using Microsoft.AspNetCore.DataProtection;
using Taskist.Core.Security;

namespace Taskist.Tests.Security;

public class SecretProtectorTests
{
    #region Utilities

    private static SecretProtector CreateProtector()
    {
        return new SecretProtector(new EphemeralDataProtectionProvider());
    }

    #endregion

    #region Protect / Unprotect

    [Fact]
    public void Protect_RoundTrips()
    {
        var protector = CreateProtector();

        Assert.Equal("token-value", protector.Unprotect(protector.Protect("token-value")));
    }

    [Fact]
    public void Unprotect_WithADifferentPurpose_ReturnsNull()
    {
        var protector = CreateProtector();
        var stored = protector.Protect("token-value", "Purpose.A");

        Assert.Null(protector.Unprotect(stored, "Purpose.B"));
    }

    [Fact]
    public void Unprotect_ValueWithoutThePrefix_IsReturnedAsIs()
    {
        Assert.Equal("legacy plain value", CreateProtector().Unprotect("legacy plain value"));
    }

    [Fact]
    public void Unprotect_LegacyEncFormat_IsNoLongerDecrypted()
    {
        Assert.Equal("enc:bGVnYWN5", CreateProtector().Unprotect("enc:bGVnYWN5"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ProtectAndUnprotect_PassEmptyValuesThrough(string? value)
    {
        var protector = CreateProtector();

        Assert.Equal(value, protector.Protect(value));
        Assert.Equal(value, protector.Unprotect(value));
    }

    #endregion

    #region Format

    [Fact]
    public void IsCurrentFormat_RecognisesProtectedValues()
    {
        var protector = CreateProtector();

        Assert.True(protector.IsCurrentFormat(protector.Protect("x")));
        Assert.True(protector.IsCurrentFormat(null));
        Assert.True(protector.IsCurrentFormat(string.Empty));
        Assert.False(protector.IsCurrentFormat("plain"));
        Assert.False(protector.IsCurrentFormat("enc:abc"));
    }

    #endregion
}
