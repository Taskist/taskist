namespace Taskist.Web.Infrastructure;

public static class TwoFactorAuthenticationExtensions
{
    #region Methods

    public static IServiceCollection AddTwoFactorSignIn(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(TwoFactorOptions.SectionName).Get<TwoFactorOptions>() ?? new TwoFactorOptions();
        services.AddSingleton(options);
        services.AddScoped<Helpers.TwoFactor.TwoFactorSetupRequiredFilter>();

        services.AddAuthentication()
            .AddCookie(TwoFactorOptions.PendingScheme, cookie =>
            {
                cookie.Cookie.Name = "Taskist.TwoFactor";
                cookie.Cookie.HttpOnly = true;
                cookie.Cookie.IsEssential = true;
                cookie.Cookie.SameSite = SameSiteMode.Lax;
                cookie.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                cookie.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                cookie.SlidingExpiration = false;
            });

        return services;
    }

    #endregion
}
