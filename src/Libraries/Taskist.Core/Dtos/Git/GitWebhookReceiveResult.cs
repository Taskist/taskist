namespace Taskist.Core.Dtos.Git;

public class GitWebhookReceiveResult
{
    public int StatusCode { get; init; }

    public string Message { get; init; } = string.Empty;

    public int? DeliveryId { get; init; }

    public static GitWebhookReceiveResult Reject(int statusCode, string message) => new() { StatusCode = statusCode, Message = message };
}
