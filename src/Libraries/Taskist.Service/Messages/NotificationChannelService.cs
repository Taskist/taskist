using System.Security.Cryptography;
using System.Text;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Taskist.Core.Common;
using Taskist.Core.Configuration;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Dtos.Notification;
using Taskist.Core.Enums;
using Taskist.Core.Extensions;
using Taskist.Data.Repository;

namespace Taskist.Service.Messages;

public class NotificationChannelService : INotificationChannelService
{
    #region Fields

    public const string ClientName = "taskist-outbound";

    private static readonly Dictionary<NotificationChannelType, string[]> AllowedHosts = new()
    {
        [NotificationChannelType.SlackChannel] = ["hooks.slack.com"],
        [NotificationChannelType.Discord] = ["discord.com", "discordapp.com"],
        [NotificationChannelType.GoogleChat] = ["chat.googleapis.com"],
        [NotificationChannelType.TeamsFlowPerson] = [".logic.azure.com", ".powerplatform.com", ".powerautomate.com"],
        [NotificationChannelType.TeamsChannel] = [".logic.azure.com", ".powerplatform.com", ".powerautomate.com"]
    };

    private readonly IRepository<NotificationChannel> _channelRepository;
    private readonly IRepository<ProjectNotificationChannel> _projectChannelRepository;
    private readonly IRepository<Project> _projectRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly BackgroundJobMode _backgroundJobMode;
    private readonly IHttpHelper _httpHelper;
    private readonly ILogger<NotificationChannelService> _logger;

    #endregion

    #region Ctor

    public NotificationChannelService(IRepository<NotificationChannel> channelRepository,
        IRepository<ProjectNotificationChannel> projectChannelRepository,
        IRepository<Project> projectRepository,
        IHttpClientFactory httpClientFactory,
        IBackgroundJobClient backgroundJobClient,
        BackgroundJobMode backgroundJobMode,
        IHttpHelper httpHelper,
        ILogger<NotificationChannelService> logger)
    {
        _channelRepository = channelRepository;
        _projectChannelRepository = projectChannelRepository;
        _projectRepository = projectRepository;
        _httpClientFactory = httpClientFactory;
        _backgroundJobClient = backgroundJobClient;
        _backgroundJobMode = backgroundJobMode;
        _httpHelper = httpHelper;
        _logger = logger;
    }

    #endregion

    #region Methods

    public async Task TaskAssignedAsync(Backlog backlog, bool tester)
    {
        var person = tester ? backlog.Tester : backlog.Assignee;
        if (backlog == null || person == null)
            return;

        await PublishAsync(backlog.ProjectId, new ChatMessage
        {
            Event = ChatEvent.TaskAssigned,
            ProjectName = backlog.Project?.Name,
            Title = $"{backlog.TaskType?.Name} #{backlog.Id} assigned to {person.Name}{(tester ? " for testing" : string.Empty)}",
            Lines = new List<string?>
            {
                backlog.Title,
                $"Project: {backlog.Project?.Name}{(backlog.Module != null ? $" / {backlog.Module.Name}" : string.Empty)}{(backlog.SubModule != null ? $" / {backlog.SubModule.Name}" : string.Empty)}",
                backlog.Severity != null ? $"Severity: {backlog.Severity.Name}" : null,
                $"Due: {(backlog.DueDate != null ? backlog.DueDate.ToDateString() : "N/A")}"
            }.Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l!).ToList(),
            Url = TaskUrl(backlog.Id),
            Recipients = [new ChatRecipient { Name = person.Name, Email = person.Email }]
        });
    }

    public async Task TaskUpdatesAsync(IList<ChatTaskUpdateDto> updates)
    {
        if (updates == null || updates.Count == 0)
            return;

        foreach (var group in updates.GroupBy(x => new { x.ProjectId, x.Event, x.TaskId }))
        {
            var first = group.First();
            var project = await _projectRepository.GetByIdAsync(first.ProjectId);
            await PublishAsync(first.ProjectId, new ChatMessage
            {
                Event = first.Event,
                ProjectName = project?.Name,
                Title = $"#{first.TaskId} {first.TaskTitle}".Trim(),
                Lines = group.Select(x => x.Message).Where(m => !string.IsNullOrWhiteSpace(m)).Distinct().ToList(),
                Url = TaskUrl(first.TaskId),
                Recipients = group.Where(x => !string.IsNullOrWhiteSpace(x.Email))
                    .GroupBy(x => x.Email, StringComparer.OrdinalIgnoreCase)
                    .Select(x => new ChatRecipient { Name = x.First().Name, Email = x.Key })
                    .ToList()
            });
        }
    }

    public async Task MergeRequestAsync(int projectId, string repositoryName, GitMergeRequestInfo mergeRequest, string authorName, User? author, User? owner)
    {
        if (mergeRequest == null)
            return;

        var state = mergeRequest.State?.ToLowerInvariant();
        var chatEvent = state switch
        {
            "merged" => ChatEvent.MergeRequestMerged,
            "closed" => ChatEvent.MergeRequestClosed,
            _ => ChatEvent.MergeRequestOpened
        };
        var recipient = chatEvent == ChatEvent.MergeRequestOpened ? owner : author;

        await PublishAsync(projectId, new ChatMessage
        {
            Event = chatEvent,
            ProjectName = repositoryName,
            Title = $"{mergeRequest.Title} ({state ?? "updated"})",
            Lines =
            [
                $"Repository: {repositoryName}",
                $"{mergeRequest.SourceBranch} → {mergeRequest.TargetBranch}",
                $"Author: {author?.Name ?? authorName}"
            ],
            Url = mergeRequest.Url,
            Recipients = recipient == null ? [] : [new ChatRecipient { Name = recipient.Name, Email = recipient.Email }]
        });
    }

    public async Task PublishAsync(int projectId, ChatMessage message)
    {
        if (projectId <= 0 || message == null || message.Event == ChatEvent.None)
            return;

        message.Url = SafeUrl(message.Url);
        var flag = (int)message.Event;
        var channelIds = await _projectChannelRepository.Table
            .Where(x => x.ProjectId == projectId && x.Active && (x.Events & flag) != 0
                        && x.NotificationChannel.Active && !x.NotificationChannel.Deleted)
            .Select(x => x.NotificationChannelId)
            .Distinct()
            .ToListAsync();

        foreach (var channelId in channelIds)
        {
            if (_backgroundJobMode.ServerEnabled)
            {
                _backgroundJobClient.Enqueue<INotificationChannelService>(svc => svc.SendAsync(channelId, message));
                continue;
            }

            try
            {
                await SendAsync(channelId, message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Sending a chat notification to channel {ChannelId} failed.", channelId);
            }
        }
    }

    public async Task SendAsync(int channelId, ChatMessage message)
    {
        var channel = await _channelRepository.GetByIdAsync(channelId);
        if (channel == null || channel.Deleted || !channel.Active)
            return;

        var (success, error) = await PostAsync(channel, message);
        channel.LastSentOn = success ? DateTime.UtcNow : channel.LastSentOn;
        channel.LastError = success ? null : Truncate(error ?? "Failed.", 1000);
        await _channelRepository.UpdateAsync(channel);

        if (!success)
            throw new InvalidOperationException($"Chat notification to channel {channelId} failed: {error}");
    }

    public async Task<(bool Success, string Message)> TestAsync(NotificationChannel channel)
    {
        var error = ValidateEndpoint(channel.ChannelType, channel.EndpointUrl);
        if (error != null)
            return (false, error);

        var (success, failure) = await PostAsync(channel, new ChatMessage
        {
            Event = ChatEvent.TaskUpdated,
            ProjectName = "Taskist",
            Title = "Taskist test message",
            Lines = ["If you can read this, the channel works."],
            Url = SafeUrl(TaskUrlBase()),
            Recipients = []
        }, testRecipient: true);

        return success ? (true, "Test message sent.") : (false, failure ?? "Failed.");
    }

    public string? ValidateEndpoint(int channelType, string? url)
    {
        if (string.IsNullOrWhiteSpace(url) || url.Length > 2000)
            return "Enter the webhook URL.";
        if (!Uri.TryCreate(url.Trim(), UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
            return "The URL must be a full https:// address.";
        if (!string.IsNullOrEmpty(uri.UserInfo))
            return "Do not put a user name or password in the URL.";

        if (AllowedHosts.TryGetValue((NotificationChannelType)channelType, out var hosts)
            && !hosts.Any(h => h.StartsWith('.') ? uri.Host.EndsWith(h, StringComparison.OrdinalIgnoreCase) : string.Equals(uri.Host, h, StringComparison.OrdinalIgnoreCase)))
            return $"This URL does not belong to the selected service ({string.Join(", ", hosts.Select(h => h.TrimStart('.')))}).";

        return null;
    }

    #endregion

    #region Channels

    public async Task<NotificationChannel?> GetChannelByIdAsync(int id)
    {
        var channel = id > 0 ? await _channelRepository.GetByIdAsync(id) : null;
        return channel == null || channel.Deleted ? null : channel;
    }

    public async Task<IList<NotificationChannel>> GetChannelsAsync(bool activeOnly = true)
    {
        return await _channelRepository.Table
            .Where(x => !x.Deleted && (!activeOnly || x.Active))
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task InsertChannelAsync(NotificationChannel channel)
    {
        channel.CreatedOn = DateTime.UtcNow;
        await _channelRepository.InsertAsync(channel);
    }

    public async Task UpdateChannelAsync(NotificationChannel channel)
    {
        channel.ModifiedOn = DateTime.UtcNow;
        await _channelRepository.UpdateAsync(channel);
    }

    public async Task DeleteChannelAsync(NotificationChannel channel)
    {
        channel.Deleted = true;
        channel.Active = false;
        channel.ModifiedOn = DateTime.UtcNow;
        await _channelRepository.UpdateAsync(channel);
    }

    #endregion

    #region Project channels

    public async Task<IList<ProjectNotificationChannel>> GetProjectChannelsAsync(int projectId)
    {
        return await _projectChannelRepository.Table
            .Include(x => x.NotificationChannel)
            .Where(x => x.ProjectId == projectId && !x.NotificationChannel.Deleted)
            .OrderBy(x => x.NotificationChannel.Name)
            .ToListAsync();
    }

    public async Task<ProjectNotificationChannel?> GetProjectChannelByIdAsync(int id)
    {
        return id > 0 ? await _projectChannelRepository.GetByIdAsync(id) : null;
    }

    public async Task<bool> ProjectChannelExistsAsync(int projectId, int channelId, int excludeId = 0)
    {
        return await _projectChannelRepository.Table
            .AnyAsync(x => x.ProjectId == projectId && x.NotificationChannelId == channelId && x.Id != excludeId);
    }

    public async Task InsertProjectChannelAsync(ProjectNotificationChannel projectChannel)
    {
        await _projectChannelRepository.InsertAsync(projectChannel);
    }

    public async Task UpdateProjectChannelAsync(ProjectNotificationChannel projectChannel)
    {
        await _projectChannelRepository.UpdateAsync(projectChannel);
    }

    public async Task DeleteProjectChannelAsync(ProjectNotificationChannel projectChannel)
    {
        await _projectChannelRepository.DeleteAsync(projectChannel);
    }

    #endregion

    #region Utilities

    private async Task<(bool Success, string? Error)> PostAsync(NotificationChannel channel, ChatMessage message, bool testRecipient = false)
    {
        var error = ValidateEndpoint(channel.ChannelType, channel.EndpointUrl);
        if (error != null)
            return (false, error);

        var type = (NotificationChannelType)channel.ChannelType;
        if (type == NotificationChannelType.TeamsFlowPerson && message.Recipients.Count == 0)
        {
            if (!testRecipient)
                return (true, null);
            return (false, "A person-to-person Teams flow needs a recipient; it can only be tested by assigning a task.");
        }

        var payloads = ChatPayloadBuilder.Build(type, message);
        if (payloads.Count == 0)
            return (true, null);

        var client = _httpClientFactory.CreateClient(ClientName);
        string? lastError = null;
        var sent = 0;

        foreach (var payload in payloads)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, channel.EndpointUrl.Trim())
                {
                    Content = new StringContent(payload, Encoding.UTF8, "application/json")
                };
                if (type == NotificationChannelType.GenericJson)
                {
                    request.Headers.Add("X-Taskist-Event", message.Event.ToString());
                    if (!string.IsNullOrWhiteSpace(channel.SigningSecret))
                        request.Headers.Add("X-Taskist-Signature-256", "sha256=" + Convert.ToHexString(
                            HMACSHA256.HashData(Encoding.UTF8.GetBytes(channel.SigningSecret), Encoding.UTF8.GetBytes(payload))).ToLowerInvariant());
                }

                using var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                    sent++;
                else
                    lastError = $"The service answered {(int)response.StatusCode} {response.ReasonPhrase}.";
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException)
            {
                lastError = $"Could not reach the service: {ex.Message}";
            }
        }

        return sent > 0 ? (true, null) : (false, lastError);
    }

    private string? TaskUrl(int taskId)
    {
        var baseUrl = TaskUrlBase();
        return baseUrl == null ? null : $"{baseUrl}/backlog/edit/{taskId}";
    }

    private string? TaskUrlBase()
    {
        try
        {
            return _httpHelper.GetBaseURL()?.TrimEnd('/');
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static string? SafeUrl(string? url)
    {
        return !string.IsNullOrWhiteSpace(url) && Uri.TryCreate(url, UriKind.Absolute, out var uri)
               && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
               && url.IndexOfAny(['|', '<', '>', '"', ' ']) < 0
            ? url
            : null;
    }

    private static string Truncate(string value, int length) => value.Length <= length ? value : value[..length];

    #endregion
}
