using System.Text.RegularExpressions;
using Taskist.Core.Security;

namespace Taskist.Tests.Security;

public class RecoveryCodesTests
{
    #region Tests

    [Fact]
    public void Generate_ReturnsTenDistinctCodes_InReadableFormat()
    {
        var codes = RecoveryCodes.Generate();

        Assert.Equal(RecoveryCodes.DefaultCount, codes.Count);
        Assert.Equal(codes.Count, codes.Distinct().Count());
        Assert.All(codes, c => Assert.Matches(new Regex("^[2-9a-z]{5}-[2-9a-z]{5}$"), c));
        Assert.All(codes, c => Assert.DoesNotContain(c, ch => ch is '0' or '1' or 'l' or 'o' or 'i'));
    }

    [Theory]
    [InlineData("ABCDE-FGHJK", "abcdefghjk")]
    [InlineData(" abcde fghjk ", "abcdefghjk")]
    [InlineData("abcdefghjk", "abcdefghjk")]
    public void Normalize_IgnoresCaseSpacesAndDashes(string input, string expected)
    {
        Assert.Equal(expected, RecoveryCodes.Normalize(input));
    }

    [Theory]
    [InlineData("abcde-fghj")]
    [InlineData("abcde-fghjkm")]
    [InlineData("abcde-fghj0")]
    [InlineData("")]
    [InlineData(null)]
    public void Normalize_RejectsMalformedCodes(string? input)
    {
        Assert.Null(RecoveryCodes.Normalize(input));
    }

    [Fact]
    public void Hash_IsStable_AndIndependentOfFormatting()
    {
        var hash = RecoveryCodes.Hash("abcde-fghjk");

        Assert.NotNull(hash);
        Assert.Equal(64, hash!.Length);
        Assert.Equal(hash, RecoveryCodes.Hash("ABCDE FGHJK"));
        Assert.NotEqual(hash, RecoveryCodes.Hash("abcde-fghjm"));
        Assert.DoesNotContain("abcde", hash);
    }

    [Fact]
    public void OtpAuthUri_FollowsTheKeyUriFormat()
    {
        var uri = OtpAuthUri.Build("Taskist", "ana@example.com", "JBSWY3DPEHPK3PXP");

        Assert.Equal("otpauth://totp/Taskist:ana%40example.com?secret=JBSWY3DPEHPK3PXP&issuer=Taskist&algorithm=SHA1&digits=6&period=30", uri);
    }

    #endregion
}
