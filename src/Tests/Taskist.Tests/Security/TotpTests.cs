using System.Text;
using Taskist.Core.Security;

namespace Taskist.Tests.Security;

public class TotpTests
{
    #region Fields

    private static readonly byte[] RfcKey = Encoding.ASCII.GetBytes("12345678901234567890");

    private static readonly string RfcSecret = Base32.Encode(RfcKey);

    #endregion

    #region Tests

    [Theory]
    [InlineData(59L, "287082")]
    [InlineData(1111111109L, "081804")]
    [InlineData(1111111111L, "050471")]
    [InlineData(1234567890L, "005924")]
    [InlineData(2000000000L, "279037")]
    [InlineData(20000000000L, "353130")]
    public void ComputeCode_MatchesRfc6238Vectors(long unixSeconds, string expected)
    {
        var step = Totp.TimeStep(DateTimeOffset.FromUnixTimeSeconds(unixSeconds));

        Assert.Equal(expected, Totp.ComputeCode(RfcKey, step));
    }

    [Fact]
    public void Verify_AcceptsCurrentCode_AndReturnsItsStep()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(1234567890);

        Assert.Equal(Totp.TimeStep(now), Totp.Verify(RfcSecret, "005924", now, 0));
    }

    [Fact]
    public void Verify_AcceptsOneStepOfClockDrift_InEitherDirection()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(1234567890);
        var previous = Totp.ComputeCode(RfcKey, Totp.TimeStep(now) - 1);
        var next = Totp.ComputeCode(RfcKey, Totp.TimeStep(now) + 1);

        Assert.NotNull(Totp.Verify(RfcSecret, previous, now, 0));
        Assert.NotNull(Totp.Verify(RfcSecret, next, now, 0));
    }

    [Fact]
    public void Verify_RejectsCodesOutsideTheWindow()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(1234567890);
        var old = Totp.ComputeCode(RfcKey, Totp.TimeStep(now) - 2);

        Assert.Null(Totp.Verify(RfcSecret, old, now, 0));
    }

    [Fact]
    public void Verify_RejectsReplayOfAnAlreadyUsedStep()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(1234567890);
        var step = Totp.Verify(RfcSecret, "005924", now, 0);

        Assert.NotNull(step);
        Assert.Null(Totp.Verify(RfcSecret, "005924", now, step!.Value));
    }

    [Fact]
    public void Verify_RejectsOlderStepAfterANewerOneWasUsed()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(1234567890);
        var previous = Totp.ComputeCode(RfcKey, Totp.TimeStep(now) - 1);

        Assert.Null(Totp.Verify(RfcSecret, previous, now, Totp.TimeStep(now)));
    }

    [Theory]
    [InlineData("000000")]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("abcdef")]
    [InlineData("")]
    [InlineData(null)]
    public void Verify_RejectsWrongOrMalformedCodes(string? code)
    {
        Assert.Null(Totp.Verify(RfcSecret, code, DateTimeOffset.FromUnixTimeSeconds(1234567890), 0));
    }

    [Fact]
    public void Verify_RejectsInvalidSecret()
    {
        Assert.Null(Totp.Verify("not base32!", "005924", DateTimeOffset.FromUnixTimeSeconds(1234567890), 0));
        Assert.Null(Totp.Verify(null, "005924", DateTimeOffset.FromUnixTimeSeconds(1234567890), 0));
    }

    [Theory]
    [InlineData("005 924", "005924")]
    [InlineData(" 005-924 ", "005924")]
    [InlineData("005924", "005924")]
    public void NormalizeCode_IgnoresSpacesAndDashes(string input, string expected)
    {
        Assert.Equal(expected, Totp.NormalizeCode(input));
    }

    [Fact]
    public void GenerateSecret_Is160BitsOfRandomBase32()
    {
        var a = Totp.GenerateSecret();
        var b = Totp.GenerateSecret();

        Assert.Equal(32, a.Length);
        Assert.Equal(Totp.SecretBytes, Base32.Decode(a)!.Length);
        Assert.NotEqual(a, b);
    }

    #endregion
}
