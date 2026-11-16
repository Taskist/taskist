using Hangfire;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Dtos.Notification;

namespace Taskist.Service.Messages;

public interface INotificationChannelService
{
    Task TaskAssignedAsync(Backlog backlog, bool tester);

    Task TaskUpdatesAsync(IList<ChatTaskUpdateDto> updates);

    Task MergeRequestAsync(int projectId, string repositoryName, GitMergeRequestInfo mergeRequest, string authorName, User? author, User? owner);

    Task PublishAsync(int projectId, ChatMessage message);

    [AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
    Task SendAsync(int channelId, ChatMessage message);

    Task<(bool Success, string Message)> TestAsync(NotificationChannel channel);

    string? ValidateEndpoint(int channelType, string? url);

    Task<NotificationChannel?> GetChannelByIdAsync(int id);

    Task<IList<NotificationChannel>> GetChannelsAsync(bool activeOnly = true);

    Task InsertChannelAsync(NotificationChannel channel);

    Task UpdateChannelAsync(NotificationChannel channel);

    Task DeleteChannelAsync(NotificationChannel channel);

    Task<IList<ProjectNotificationChannel>> GetProjectChannelsAsync(int projectId);

    Task<ProjectNotificationChannel?> GetProjectChannelByIdAsync(int id);

    Task<bool> ProjectChannelExistsAsync(int projectId, int channelId, int excludeId = 0);

    Task InsertProjectChannelAsync(ProjectNotificationChannel projectChannel);

    Task UpdateProjectChannelAsync(ProjectNotificationChannel projectChannel);

    Task DeleteProjectChannelAsync(ProjectNotificationChannel projectChannel);
}
