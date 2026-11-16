using Taskist.Core.Enums;
using Taskist.Service.WorkItems;

namespace Taskist.Tests.WorkItems;

public class ReleaseVersionTemplateTests
{
    #region Utilities

    private static readonly DateOnly Today = new(2026, 10, 20);

    private static string Next(string format, string[] existing, ReleaseType type,
        ReleaseVersionPart fullIncrement = ReleaseVersionPart.Minor, string? baseName = null)
    {
        return ReleaseVersionTemplate.Next(format, existing, type, fullIncrement, Today, baseName);
    }

    #endregion

    #region First release

    [Fact]
    public void Next_FirstFullRelease_IsOnePointZero()
    {
        Assert.Equal("1.0.0", Next(ReleaseVersionTemplate.DefaultFormat, [], ReleaseType.Full));
    }

    [Fact]
    public void Next_FirstPatchRelease_IsZeroZeroOne()
    {
        Assert.Equal("0.0.1", Next(ReleaseVersionTemplate.DefaultFormat, [], ReleaseType.Patch));
    }

    #endregion

    #region Bumping

    [Fact]
    public void Next_FullWithMinorIncrement_BumpsMinorAndResetsPatch()
    {
        Assert.Equal("1.3.0", Next(ReleaseVersionTemplate.DefaultFormat, ["1.1.0", "1.2.3"], ReleaseType.Full));
    }

    [Fact]
    public void Next_FullWithMajorIncrement_BumpsMajorAndResetsTheRest()
    {
        Assert.Equal("2.0.0", Next(ReleaseVersionTemplate.DefaultFormat, ["1.2.3"], ReleaseType.Full, ReleaseVersionPart.Major));
    }

    [Fact]
    public void Next_Patch_BumpsTheHighestExistingVersion()
    {
        Assert.Equal("1.5.1", Next(ReleaseVersionTemplate.DefaultFormat, ["1.2.3", "1.5.0", "1.4.9"], ReleaseType.Patch));
    }

    [Fact]
    public void Next_ComparesCountersNumericallyNotAlphabetically()
    {
        Assert.Equal("1.10.1", Next(ReleaseVersionTemplate.DefaultFormat, ["1.9.0", "1.10.0"], ReleaseType.Patch));
    }

    [Fact]
    public void Next_IgnoresNamesThatDoNotMatchTheFormat()
    {
        Assert.Equal("1.1.0", Next(ReleaseVersionTemplate.DefaultFormat, ["Sprint 5", "9.9.9-hotfix", "", "1.0.0"], ReleaseType.Full));
    }

    [Fact]
    public void Next_WithLiteralText_KeepsItAndMatchesCaseInsensitively()
    {
        Assert.Equal("v1.5", Next("v{major}.{minor}", ["V1.4"], ReleaseType.Full));
    }

    [Fact]
    public void Next_WithAnInvalidFormat_FallsBackToTheDefault()
    {
        Assert.Equal("1.0.0", Next("{bogus}", [], ReleaseType.Full));
    }

    #endregion

    #region Patch of a base release

    [Fact]
    public void Next_PatchOfABase_StaysOnTheBaseLine()
    {
        Assert.Equal("1.2.4", Next(ReleaseVersionTemplate.DefaultFormat, ["1.2.3", "1.5.0"], ReleaseType.Patch, baseName: "1.2.0"));
    }

    [Fact]
    public void Next_PatchOfABaseWithoutPatches_StartsAtOne()
    {
        Assert.Equal("1.5.1", Next(ReleaseVersionTemplate.DefaultFormat, ["1.2.3", "1.5.0"], ReleaseType.Patch, baseName: "1.5.0"));
    }

    #endregion

    #region Date tokens

    [Fact]
    public void Next_WithDateTokens_CountsOnlyTodaysPeriod()
    {
        Assert.Equal("2026.10.4", Next("{yyyy}.{MM}.{seq}", ["2026.10.3", "2026.09.7"], ReleaseType.Patch));
    }

    [Fact]
    public void Next_WithDateTokens_ResetsSequenceInANewPeriod()
    {
        Assert.Equal("2026.10.1", Next("{yyyy}.{MM}.{seq}", ["2026.09.7"], ReleaseType.Patch));
    }

    #endregion

    #region Validation

    [Theory]
    [InlineData("{major}.{minor}.{patch}")]
    [InlineData("v{major}")]
    [InlineData("{MAJOR}.{Minor}")]
    [InlineData("{yyyy}.{MM}.{dd}-{seq}")]
    [InlineData("rel-{yy}{MM}.{seq}")]
    public void IsValid_AcceptsFormatsWithACounter(string format)
    {
        Assert.True(ReleaseVersionTemplate.IsValid(format));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("{yyyy}.{MM}")]
    [InlineData("{major}.{major}")]
    [InlineData("{nope}")]
    [InlineData("{mm}.{seq}")]
    [InlineData("{major}}")]
    [InlineData("{major}.{minor}.{patch}-012345678901234567890123456789012345")]
    public void IsValid_RejectsBadFormats(string? format)
    {
        Assert.False(ReleaseVersionTemplate.IsValid(format));
    }

    [Theory]
    [InlineData("1.2.3", true)]
    [InlineData(" 1.2.3 ", true)]
    [InlineData("1.2", false)]
    [InlineData("1.2.3.4", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void Matches_ChecksTheWholeName(string? name, bool expected)
    {
        Assert.Equal(expected, ReleaseVersionTemplate.Matches(ReleaseVersionTemplate.DefaultFormat, name));
    }

    #endregion
}
