namespace Taskist.Service.Git;

public interface IGitLegacyUpgradeService
{
    Task<int> UpgradeAsync();

    Task<int> ReEncryptSecretsAsync();
}
