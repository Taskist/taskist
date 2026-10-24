using Taskist.Service.Git;

namespace Taskist.Web.Infrastructure;

public static class GitStartup
{
    public static async Task UpgradeLegacySettingsAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(GitStartup));

        try
        {
            await scope.ServiceProvider.GetRequiredService<IGitLegacyUpgradeService>().UpgradeAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Upgrading the legacy GitLab settings failed. Git connections can be configured manually under Configuration → Git connections.");
        }
    }
}
