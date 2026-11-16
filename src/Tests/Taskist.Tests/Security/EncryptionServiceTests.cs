using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Taskist.Core.Enums;
using Taskist.Core.Security;
using Taskist.Service.Security;

namespace Taskist.Tests.Security;

public class EncryptionServiceTests
{
    #region Utilities

    private static EncryptionService CreateService()
    {
        return new EncryptionService(new SecretProtector(new EphemeralDataProtectionProvider()));
    }

    private static string HashWithIterations(string password, int iterations)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(Encoding.UTF8.GetBytes(password), salt, iterations, HashAlgorithmName.SHA512, 64);

        return $"pbkdf2-sha512${iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
    }

    #endregion

    #region Password hashing

    [Fact]
    public void HashPassword_UsesTheSelfDescribingPbkdf2Format()
    {
        var parts = CreateService().HashPassword("correct horse").Split('$');

        Assert.Equal(4, parts.Length);
        Assert.Equal("pbkdf2-sha512", parts[0]);
        Assert.Equal("210000", parts[1]);
        Assert.Equal(16, Convert.FromBase64String(parts[2]).Length);
        Assert.Equal(64, Convert.FromBase64String(parts[3]).Length);
    }

    [Fact]
    public void HashPassword_UsesAFreshSaltEachTime()
    {
        var service = CreateService();

        var hashes = Enumerable.Range(0, 5).Select(_ => service.HashPassword("same password")).ToList();

        Assert.Equal(hashes.Count, hashes.Distinct().Count());
    }

    [Fact]
    public void HashPassword_WithNull_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => CreateService().HashPassword(null!));
    }

    #endregion

    #region Password verification

    [Fact]
    public void VerifyPassword_WithCorrectPassword_Succeeds()
    {
        var service = CreateService();

        Assert.Equal(PasswordCheckResult.Success, service.VerifyPassword("s3cret", service.HashPassword("s3cret")));
    }

    [Fact]
    public void VerifyPassword_WithWrongPassword_Fails()
    {
        var service = CreateService();

        Assert.Equal(PasswordCheckResult.Failed, service.VerifyPassword("not the password", service.HashPassword("s3cret")));
    }

    [Fact]
    public void VerifyPassword_IsCaseSensitive()
    {
        var service = CreateService();

        Assert.Equal(PasswordCheckResult.Failed, service.VerifyPassword("casesensitive", service.HashPassword("CaseSensitive")));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("plain-text-password")]
    [InlineData("pbkdf2-sha512$210000$only-three")]
    [InlineData("pbkdf2-sha512$abc$c2FsdA==$aGFzaA==")]
    [InlineData("pbkdf2-sha512$210000$not-base64!!$aGFzaA==")]
    [InlineData("pbkdf2-sha256$210000$c2FsdA==$aGFzaA==")]
    public void VerifyPassword_WithMalformedStoredHash_FailsRatherThanThrows(string? storedHash)
    {
        Assert.Equal(PasswordCheckResult.Failed, CreateService().VerifyPassword("password", storedHash!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void VerifyPassword_WithEmptyPassword_Fails(string? password)
    {
        var service = CreateService();

        Assert.Equal(PasswordCheckResult.Failed, service.VerifyPassword(password!, service.HashPassword("x")));
    }

    [Fact]
    public void VerifyPassword_BelowTheMinimumIterationCount_Fails()
    {
        Assert.Equal(PasswordCheckResult.Failed, CreateService().VerifyPassword("password", HashWithIterations("password", 9999)));
    }

    [Fact]
    public void VerifyPassword_WithOlderIterationCount_AsksForARehash()
    {
        Assert.Equal(PasswordCheckResult.SuccessRehashNeeded, CreateService().VerifyPassword("password", HashWithIterations("password", 100000)));
    }

    #endregion

    #region Text encryption

    [Fact]
    public void EncryptText_RoundTrips()
    {
        var service = CreateService();

        Assert.Equal("attack at dawn", service.DecryptText(service.EncryptText("attack at dawn")));
    }

    [Fact]
    public void EncryptText_ProducesTheCurrentFormat()
    {
        Assert.StartsWith(SecretProtector.Prefix, CreateService().EncryptText("value"));
    }

    [Fact]
    public void EncryptText_ProducesDifferentCiphertextForTheSameInput()
    {
        var service = CreateService();

        Assert.NotEqual(service.EncryptText("identical input"), service.EncryptText("identical input"));
    }

    [Fact]
    public void DecryptText_WithAnotherKeyRing_ReturnsNull()
    {
        var cipher = CreateService().EncryptText("confidential");

        Assert.Null(CreateService().DecryptText(cipher));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("plain value")]
    [InlineData("enc:bGVnYWN5")]
    public void DecryptText_WithoutTheCurrentPrefix_ReturnsNull(string? cipher)
    {
        Assert.Null(CreateService().DecryptText(cipher!));
    }

    [Fact]
    public void DecryptText_WithATamperedPayload_ReturnsNull()
    {
        var service = CreateService();
        var cipher = service.EncryptText("value");

        Assert.Null(service.DecryptText(cipher[..^4] + "AAAA"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void EncryptText_WithEmptyInput_ReturnsInput(string? input)
    {
        Assert.Equal(input, CreateService().EncryptText(input!));
    }

    #endregion

    #region Tokens

    [Fact]
    public void GenerateToken_ProducesAUniqueTokenPerCall()
    {
        var service = CreateService();
        var code = Guid.NewGuid();

        var tokens = Enumerable.Range(0, 10).Select(_ => service.GenerateToken(code)).ToList();

        Assert.Equal(tokens.Count, tokens.Distinct().Count());
    }

    [Fact]
    public void GenerateToken_EmbedsTheUserCode()
    {
        var service = CreateService();
        var code = Guid.NewGuid();

        var decrypted = service.DecryptText(service.GenerateToken(code));

        Assert.Equal(code.ToString(), decrypted.Split('|')[0]);
    }

    [Fact]
    public void ValidateToken_AcceptsAFreshToken()
    {
        var service = CreateService();
        var timestampPart = service.DecryptText(service.GenerateToken(Guid.NewGuid())).Split('|')[1];

        Assert.True(service.ValidateToken(timestampPart, 10));
    }

    [Fact]
    public void ValidateToken_RejectsAnExpiredToken()
    {
        var service = CreateService();
        var timestampPart = service.DecryptText(service.GenerateToken(Guid.NewGuid())).Split('|')[1];

        Assert.False(service.ValidateToken(timestampPart, 0));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-base64-!!!")]
    [InlineData("AAA=")]
    public void ValidateToken_RejectsMalformedInputWithoutThrowing(string? token)
    {
        Assert.False(CreateService().ValidateToken(token!));
    }

    #endregion
}
