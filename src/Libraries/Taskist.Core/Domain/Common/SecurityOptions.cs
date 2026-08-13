namespace Taskist.Core.Domain.Common;

/// <summary>
/// Security related settings bound from the "Security" configuration section.
/// </summary>
public class SecurityOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Security";

    /// <summary>
    /// The key that was hard-coded in earlier versions. Deployments still carrying this
    /// value are rejected at startup because it is published in the public repository.
    /// </summary>
    public const string CompromisedKey = "E546C8DF278CD5931069B522E695D4F2";

    /// <summary>
    /// Minimum accepted key length.
    /// </summary>
    public const int MinimumKeyLength = 32;

    /// <summary>
    /// Symmetric key used to protect activation / reset tokens.
    /// Supply through configuration or the TASKIST_Security__EncryptionKey environment variable.
    /// </summary>
    public string EncryptionKey { get; set; } = string.Empty;

    /// <summary>
    /// Number of consecutive failed sign-in attempts before an account is locked.
    /// </summary>
    public int MaxFailedAccessAttempts { get; set; } = 5;

    /// <summary>
    /// How long an account stays locked once <see cref="MaxFailedAccessAttempts"/> is reached.
    /// </summary>
    public int LockoutMinutes { get; set; } = 15;

    /// <summary>
    /// Whether authentication and session cookies are restricted to HTTPS.
    /// <para>
    /// Leave enabled. Set to false only when deliberately serving over plain HTTP,
    /// such as a local container evaluation, because browsers withhold secure
    /// cookies on HTTP and sign-in then fails with no visible error.
    /// </para>
    /// </summary>
    public bool RequireHttpsCookies { get; set; } = true;

    /// <summary>
    /// Validates the options and throws when the deployment is not safely configured.
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(EncryptionKey))
            throw new InvalidOperationException(
                "Security:EncryptionKey is not configured. Generate a unique key (for example " +
                "`openssl rand -base64 32`) and supply it through configuration, user-secrets or the " +
                "TASKIST_Security__EncryptionKey environment variable. Taskist will not start without it.");

        if (EncryptionKey.Equals(CompromisedKey, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Security:EncryptionKey is set to the key that shipped in earlier public releases. " +
                "It is published in the source repository and provides no protection. Generate a unique key.");

        if (EncryptionKey.Length < MinimumKeyLength)
            throw new InvalidOperationException(
                $"Security:EncryptionKey must be at least {MinimumKeyLength} characters long.");

        if (MaxFailedAccessAttempts <= 0)
            throw new InvalidOperationException("Security:MaxFailedAccessAttempts must be greater than zero.");

        if (LockoutMinutes <= 0)
            throw new InvalidOperationException("Security:LockoutMinutes must be greater than zero.");
    }
}
