using Taskist.Core.Dtos.Git;

namespace Taskist.Service.Git;

public interface IGitHubAppTokenService
{
    string CreateAppJwt(string appId, string privateKeyPem);

    Task<GitResult<string>> GetInstallationTokenAsync(int connectionId, string appId, string privateKeyPem, Uri apiBase, HttpClient http, string? owner);

    void Forget(int connectionId);
}
