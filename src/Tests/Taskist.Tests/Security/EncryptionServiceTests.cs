using System.Security.Cryptography;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Users;
using Taskist.Service.Security;
using Xunit;

namespace Taskist.Tests.Security;

public class EncryptionServiceTests
{
    #region Utilities

    private const string TestKey = "unit-test-encryption-key-32-chars!!";

    private static EncryptionService CreateService(string key = TestKey)
    {
        return new EncryptionService(Options.Create(new SecurityOptions { EncryptionKey = key }));
    }

    #endregion

    #region Password hashing

    [Fact]
    public void CreatePasswordHash_WithSameSalt_IsDeterministic()
    {
        var service = CreateService();
        var salt = service.CreatePasswordSalt();

        var first = service.CreatePasswordHash("correct horse", salt, PasswordFormat.Pbkdf2);
        var second = service.CreatePasswordHash("correct horse", salt, PasswordFormat.Pbkdf2);

        second.Should().Be(first);
    }

    [Fact]
    public void CreatePasswordHash_WithDifferentSalts_ProducesDifferentHashes()
    {
        var service = CreateService();

        var first = service.CreatePasswordHash("same password", service.CreatePasswordSalt(), PasswordFormat.Pbkdf2);
        var second = service.CreatePasswordHash("same password", service.CreatePasswordSalt(), PasswordFormat.Pbkdf2);

        second.Should().NotBe(first);
    }

    [Fact]
    public void CreatePasswordHash_DefaultOverload_UsesPbkdf2()
    {
        var service = CreateService();
        var salt = service.CreatePasswordSalt();

        var viaDefault = service.CreatePasswordHash("hunter2", salt);
        var viaPbkdf2 = service.CreatePasswordHash("hunter2", salt, PasswordFormat.Pbkdf2);

        viaDefault.Should().Be(viaPbkdf2);
    }

    [Fact]
    public void CreatePasswordSalt_ReturnsANewValueEachTime()
    {
        var service = CreateService();

        var salts = Enumerable.Range(0, 25).Select(_ => service.CreatePasswordSalt()).ToList();

        salts.Should().OnlyHaveUniqueItems();
    }

    #endregion

    #region Password verification

    [Fact]
    public void VerifyPassword_WithCorrectPassword_Succeeds()
    {
        var service = CreateService();
        var salt = service.CreatePasswordSalt();
        var hash = service.CreatePasswordHash("s3cret", salt, PasswordFormat.Pbkdf2);

        service.VerifyPassword("s3cret", salt, hash, PasswordFormat.Pbkdf2).Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithWrongPassword_Fails()
    {
        var service = CreateService();
        var salt = service.CreatePasswordSalt();
        var hash = service.CreatePasswordHash("s3cret", salt, PasswordFormat.Pbkdf2);

        service.VerifyPassword("not the password", salt, hash, PasswordFormat.Pbkdf2).Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_IsCaseSensitive()
    {
        var service = CreateService();
        var salt = service.CreatePasswordSalt();
        var hash = service.CreatePasswordHash("CaseSensitive", salt, PasswordFormat.Pbkdf2);

        service.VerifyPassword("casesensitive", salt, hash, PasswordFormat.Pbkdf2).Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithNullInput_FailsRatherThanThrows()
    {
        var service = CreateService();
        var salt = service.CreatePasswordSalt();
        var hash = service.CreatePasswordHash("whatever", salt, PasswordFormat.Pbkdf2);

        service.VerifyPassword(null!, salt, hash, PasswordFormat.Pbkdf2).Should().BeFalse();
        service.VerifyPassword("whatever", null!, hash, PasswordFormat.Pbkdf2).Should().BeFalse();
        service.VerifyPassword("whatever", salt, null!, PasswordFormat.Pbkdf2).Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithCorruptSalt_FailsRatherThanThrows()
    {
        var service = CreateService();

        //a salt that is not valid base64 must not surface as an unhandled exception
        var act = () => service.VerifyPassword("password", "not-base64-!!", "hash", PasswordFormat.Pbkdf2);

        act.Should().NotThrow();
        act().Should().BeFalse();
    }

    #endregion

    #region Legacy hash compatibility

    [Fact]
    public void VerifyPassword_AcceptsLegacySha1Hashes()
    {
        //credentials created before the PBKDF2 migration must keep working
        var service = CreateService();
        var salt = "n1DRhPvrjpXMEA==";
        var legacyHash = service.CreatePasswordHash("legacy password", salt, PasswordFormat.Sha1Legacy);

        service.VerifyPassword("legacy password", salt, legacyHash, PasswordFormat.Sha1Legacy)
            .Should().BeTrue();
    }

    [Fact]
    public void LegacyAndPbkdf2_ProduceDifferentHashes()
    {
        var service = CreateService();
        var salt = service.CreatePasswordSalt();

        var legacy = service.CreatePasswordHash("password", salt, PasswordFormat.Sha1Legacy);
        var modern = service.CreatePasswordHash("password", salt, PasswordFormat.Pbkdf2);

        modern.Should().NotBe(legacy);
    }

    [Fact]
    public void VerifyPassword_DoesNotAcceptLegacyHashUnderPbkdf2Format()
    {
        var service = CreateService();
        var salt = service.CreatePasswordSalt();
        var legacyHash = service.CreatePasswordHash("password", salt, PasswordFormat.Sha1Legacy);

        service.VerifyPassword("password", salt, legacyHash, PasswordFormat.Pbkdf2).Should().BeFalse();
    }

    #endregion

    #region Text encryption

    [Fact]
    public void EncryptText_RoundTrips()
    {
        var service = CreateService();

        var cipher = service.EncryptText("attack at dawn");

        service.DecryptText(cipher).Should().Be("attack at dawn");
    }

    [Fact]
    public void EncryptText_UsesAFreshIvPerCall()
    {
        //a fixed IV leaks that two ciphertexts hold the same plaintext
        var service = CreateService();

        var first = service.EncryptText("identical input");
        var second = service.EncryptText("identical input");

        second.Should().NotBe(first);
        service.DecryptText(first).Should().Be(service.DecryptText(second));
    }

    [Fact]
    public void DecryptText_WithADifferentKey_DoesNotReturnThePlaintext()
    {
        var cipher = CreateService().EncryptText("confidential");
        var otherService = CreateService("a-completely-different-key-32chars!!");

        var act = () => otherService.DecryptText(cipher);

        //either the padding check fails or the output is garbage; never the original
        try
        {
            act().Should().NotBe("confidential");
        }
        catch (CryptographicException)
        {
            //expected outcome for an authenticated failure
        }
    }

    [Fact]
    public void DecryptText_WithMalformedPayload_Throws()
    {
        var service = CreateService();

        var act = () => service.DecryptText(Convert.ToBase64String([0x02, 0x03]));

        act.Should().Throw<CryptographicException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void EncryptText_WithEmptyInput_ReturnsInput(string? input)
    {
        CreateService().EncryptText(input!).Should().Be(input);
    }

    #endregion

    #region Tokens

    [Fact]
    public void GenerateToken_ProducesAUniqueTokenPerCall()
    {
        var service = CreateService();
        var code = Guid.NewGuid();

        var tokens = Enumerable.Range(0, 10).Select(_ => service.GenerateToken(code)).ToList();

        tokens.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void GenerateToken_EmbedsTheUserCode()
    {
        var service = CreateService();
        var code = Guid.NewGuid();

        var decrypted = service.DecryptText(service.GenerateToken(code));

        decrypted.Split('|').First().Should().Be(code.ToString());
    }

    [Fact]
    public void ValidateToken_AcceptsAFreshToken()
    {
        var service = CreateService();
        var decrypted = service.DecryptText(service.GenerateToken(Guid.NewGuid()));
        var timestampPart = decrypted.Split('|')[1];

        service.ValidateToken(timestampPart, 10).Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_RejectsAnExpiredToken()
    {
        var service = CreateService();
        var decrypted = service.DecryptText(service.GenerateToken(Guid.NewGuid()));
        var timestampPart = decrypted.Split('|')[1];

        //a zero minute window means anything already issued has expired
        service.ValidateToken(timestampPart, 0).Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("not-base64-!!!")]
    [InlineData("AAA=")]
    public void ValidateToken_RejectsMalformedInputWithoutThrowing(string? token)
    {
        var service = CreateService();

        var act = () => service.ValidateToken(token!);

        act.Should().NotThrow();
        act().Should().BeFalse();
    }

    #endregion
}
