using Taskist.Core.Dtos.Git;

namespace Taskist.Service.Git;

public interface IGitWebhookParser
{
    string? EventName(Func<string, string?> header);

    string? DeliveryId(Func<string, string?> header);

    bool Verify(Func<string, string?> header, byte[] body, string secret);

    GitWebhookEvent Parse(string eventName, string body);
}
