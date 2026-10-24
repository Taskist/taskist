using Taskist.Core.Domain.Git;

namespace Taskist.Service.Git;

public interface IGitProviderFactory
{
    IGitProvider Create(GitConnection connection);

    IGitWebhookParser CreateWebhookParser(int providerType);

    string DefaultApiUrl(int providerType, string baseUrl);
}
