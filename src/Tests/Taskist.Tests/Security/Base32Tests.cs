using System.Text;
using Taskist.Core.Security;

namespace Taskist.Tests.Security;

public class Base32Tests
{
    #region Tests

    [Theory]
    [InlineData("f", "MY")]
    [InlineData("fo", "MZXQ")]
    [InlineData("foo", "MZXW6")]
    [InlineData("foob", "MZXW6YQ")]
    [InlineData("fooba", "MZXW6YTB")]
    [InlineData("foobar", "MZXW6YTBOI")]
    public void Encode_MatchesRfc4648Vectors_WithoutPadding(string input, string expected)
    {
        Assert.Equal(expected, Base32.Encode(Encoding.ASCII.GetBytes(input)));
    }

    [Theory]
    [InlineData("MZXW6YTBOI")]
    [InlineData("mzxw6ytboi")]
    [InlineData("MZXW 6YTB OI")]
    [InlineData("MZXW6YTBOI======")]
    public void Decode_IsCaseInsensitive_AndIgnoresSpacesAndPadding(string input)
    {
        Assert.Equal("foobar", Encoding.ASCII.GetString(Base32.Decode(input)!));
    }

    [Theory]
    [InlineData("MZXW1")]
    [InlineData("MZXW8")]
    [InlineData("")]
    [InlineData(null)]
    public void Decode_RejectsInvalidInput(string? input)
    {
        Assert.Null(Base32.Decode(input));
    }

    [Fact]
    public void Group_SplitsIntoFours()
    {
        Assert.Equal("ABCD EFGH IJ", Base32.Group("ABCDEFGHIJ"));
    }

    #endregion
}
