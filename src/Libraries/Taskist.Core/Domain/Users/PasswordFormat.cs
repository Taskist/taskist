namespace Taskist.Core.Domain.Users;

/// <summary>
/// Algorithm used to derive a stored password hash.
/// </summary>
public enum PasswordFormat
{
    /// <summary>
    /// Legacy single-pass SHA1 over (password + salt). Retained only so existing
    /// credentials keep working; such hashes are upgraded to <see cref="Pbkdf2"/>
    /// on the next successful sign-in.
    /// </summary>
    Sha1Legacy = 0,

    /// <summary>
    /// PBKDF2-HMAC-SHA256. The format used for all newly created passwords.
    /// </summary>
    Pbkdf2 = 1
}
