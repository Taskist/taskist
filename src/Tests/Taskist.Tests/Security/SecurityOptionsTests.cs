using FluentAssertions;
using Taskist.Core.Domain.Common;
using Xunit;

namespace Taskist.Tests.Security;

public class SecurityOptionsTests
{
    #region Utilities

    private static SecurityOptions Valid() => new()
    {
        EncryptionKey = "a-perfectly-fine-encryption-key-32!!",
        MaxFailedAccessAttempts = 5,
        LockoutMinutes = 15
    };

    #endregion

    #region Encryption key

    [Fact]
    public void Validate_WithAGoodKey_DoesNotThrow()
    {
        var act = () => Valid().Validate();

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_WithoutAKey_Throws(string? key)
    {
        var options = Valid();
        options.EncryptionKey = key!;

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not configured*");
    }

    [Fact]
    public void Validate_WithThePublishedKey_Throws()
    {
        //this key shipped in earlier public releases and offers no protection
        var options = Valid();
        options.EncryptionKey = SecurityOptions.CompromisedKey;

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*earlier public releases*");
    }

    [Fact]
    public void Validate_WithThePublishedKeyInADifferentCase_StillThrows()
    {
        var options = Valid();
        options.EncryptionKey = SecurityOptions.CompromisedKey.ToLowerInvariant();

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Validate_WithAShortKey_Throws()
    {
        var options = Valid();
        options.EncryptionKey = new string('k', SecurityOptions.MinimumKeyLength - 1);

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*at least {SecurityOptions.MinimumKeyLength}*");
    }

    [Fact]
    public void Validate_AtExactlyTheMinimumKeyLength_DoesNotThrow()
    {
        var options = Valid();
        options.EncryptionKey = new string('k', SecurityOptions.MinimumKeyLength);

        var act = () => options.Validate();

        act.Should().NotThrow();
    }

    #endregion

    #region Lockout

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_WithANonPositiveAttemptLimit_Throws(int attempts)
    {
        var options = Valid();
        options.MaxFailedAccessAttempts = attempts;

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*MaxFailedAccessAttempts*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_WithANonPositiveLockoutWindow_Throws(int minutes)
    {
        var options = Valid();
        options.LockoutMinutes = minutes;

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*LockoutMinutes*");
    }

    #endregion

    #region Defaults

    [Fact]
    public void RequireHttpsCookies_DefaultsToEnabled()
    {
        new SecurityOptions().RequireHttpsCookies.Should().BeTrue();
    }

    #endregion
}
