using Taskist.Core.Security;

namespace Taskist.Web.Infrastructure;

public static class StartupDiagnostics
{
    public static void LogSecurityWarnings(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
            return;

        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(StartupDiagnostics));
        var security = app.Services.GetRequiredService<HostSecurityOptions>();
        var keyRing = app.Services.GetRequiredService<KeyRingProtectionInfo>();

        if (!security.RequireHttpsCookies)
            logger.LogWarning("Security:RequireHttpsCookies is false, so sign-in cookies are also sent over plain HTTP. Put Taskist behind TLS and set it to true.");

        if (!keyRing.ProtectedAtRest)
            logger.LogWarning("The Data Protection key ring at {KeysPath} is not encrypted at rest. Anyone who can read it can decrypt stored tokens and secrets. Configure DataProtection:CertificatePath, CertificateThumbprint or ProtectWithDpapi, and back the folder up.", keyRing.KeysPath);

        if (security.ForwardedHeaders.Enabled && security.ForwardedHeaders.KnownProxies.Length == 0 && security.ForwardedHeaders.KnownNetworks.Length == 0)
            logger.LogInformation("Forwarded headers are enabled but no Security:ForwardedHeaders:KnownProxies or KnownNetworks are set, so only a proxy on loopback is trusted.");
    }

    public static void LogErrorTrackingStatus(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(StartupDiagnostics));
        var errorTracking = app.Services.GetRequiredService<ErrorTrackingOptions>();

        if (errorTracking.IsMisconfigured)
            logger.LogWarning("Sentry:Enabled is true but Sentry:Dsn is missing or not a valid DSN, so error tracking is off. Copy the DSN from your Sentry project settings.");
        else if (errorTracking.IsActive)
            logger.LogInformation("Error tracking is on: unhandled exceptions and errors are sent to Sentry (traces sample rate {Rate}).", errorTracking.EffectiveTracesSampleRate);
    }

    public static void LogSingleSignOnStatus(this WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(StartupDiagnostics));
        var oidc = app.Services.GetRequiredService<OidcOptions>();

        if (oidc.IsMisconfigured)
        {
            logger.LogWarning("Authentication:Oidc:Enabled is true but Authority or ClientId is missing or invalid, so single sign-on is off. Authority must be an https URL (http only with RequireHttpsMetadata false).");
            return;
        }

        if (!oidc.IsActive)
            return;

        logger.LogInformation("Single sign-on is on ({DisplayName}, {Authority}). Auto-provisioning: {AutoProvision}. Password sign-in limited to administrators: {Restricted}.",
            oidc.EffectiveDisplayName, oidc.Authority, oidc.AutoProvision, oidc.LocalLoginRestricted);

        if (!oidc.RequireHttpsMetadata)
            logger.LogWarning("Authentication:Oidc:RequireHttpsMetadata is false. Only use this for local testing; in production the identity provider must be reached over HTTPS.");

        if (!oidc.RequireVerifiedEmail)
            logger.LogWarning("Authentication:Oidc:RequireVerifiedEmail is false, so an SSO sign-in can be linked to an existing Taskist account by e-mail even when the identity provider has not verified that address. Only use this with a provider whose e-mail addresses you control.");
    }
}
