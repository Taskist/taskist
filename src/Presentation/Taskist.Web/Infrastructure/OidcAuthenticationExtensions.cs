using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Taskist.Web.Helpers.Sso;

namespace Taskist.Web.Infrastructure;

public static class OidcAuthenticationExtensions
{
    #region Methods

    public static IServiceCollection AddOidcSignIn(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(OidcOptions.SectionName).Get<OidcOptions>() ?? new OidcOptions();
        services.AddSingleton(options);
        services.AddScoped<ISsoSignInService, SsoSignInService>();

        if (!options.IsActive)
            return services;

        services.AddAuthentication()
            .AddCookie(OidcOptions.ExternalCookieScheme, cookie =>
            {
                cookie.Cookie.Name = "Taskist.External";
                cookie.Cookie.HttpOnly = true;
                cookie.Cookie.IsEssential = true;
                cookie.Cookie.SameSite = SameSiteMode.Lax;
                cookie.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                cookie.ExpireTimeSpan = TimeSpan.FromMinutes(10);
                cookie.SlidingExpiration = false;
            })
            .AddOpenIdConnect(OidcOptions.SchemeName, oidc =>
            {
                oidc.SignInScheme = OidcOptions.ExternalCookieScheme;
                oidc.Authority = options.Authority!.Trim();
                oidc.ClientId = options.ClientId!.Trim();
                if (!string.IsNullOrWhiteSpace(options.ClientSecret))
                    oidc.ClientSecret = options.ClientSecret.Trim();

                oidc.RequireHttpsMetadata = options.RequireHttpsMetadata;
                oidc.ResponseType = OpenIdConnectResponseType.Code;
                oidc.ResponseMode = OpenIdConnectResponseMode.Query;
                oidc.UsePkce = true;
                oidc.SaveTokens = false;
                oidc.GetClaimsFromUserInfoEndpoint = true;
                oidc.MapInboundClaims = false;
                oidc.CallbackPath = OidcOptions.CallbackPath;
                oidc.TokenValidationParameters.NameClaimType = "name";

                oidc.Scope.Clear();
                foreach (var scope in options.EffectiveScopes)
                    oidc.Scope.Add(scope);

                oidc.ClaimActions.MapUniqueJsonKey("email_verified", "email_verified");
                if (options.EffectiveRoleClaim != null)
                    oidc.ClaimActions.MapJsonKey(options.EffectiveRoleClaim, options.EffectiveRoleClaim);

                oidc.CorrelationCookie.SameSite = SameSiteMode.Lax;
                oidc.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                oidc.NonceCookie.SameSite = SameSiteMode.Lax;
                oidc.NonceCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

                oidc.Events = new OpenIdConnectEvents
                {
                    OnRemoteFailure = context => RedirectToLogin(context, SsoErrorCode.Failed),
                    OnAccessDenied = context => RedirectToLogin(context, SsoErrorCode.Denied)
                };
            });

        return services;
    }

    #endregion

    #region Utilities

    private static Task RedirectToLogin(HandleRequestContext<RemoteAuthenticationOptions> context, string code)
    {
        if (context is RemoteFailureContext failure && failure.Failure != null)
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(OidcAuthenticationExtensions));
            logger.LogWarning("Single sign-on failed: {Reason}", failure.Failure.Message);
        }

        context.Response.Redirect($"{context.Request.PathBase}/Login?sso={code}");
        context.HandleResponse();
        return Task.CompletedTask;
    }

    #endregion
}
