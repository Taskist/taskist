using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Taskist.Core.Domain.Git;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Enums;
using Taskist.Data.Repository;
using Taskist.Service.Git.Issues;
using Taskist.Service.Messages;
using Taskist.Service.WorkItems;

namespace Taskist.Service.Git;

public class GitWebhookService : IGitWebhookService
{
    #region Fields

    private const int MaxBodyBytes = 5 * 1024 * 1024;
    private const int ErrorLength = 1000;

    private readonly IRepository<GitWebhookDelivery> _deliveryRepository;
    private readonly IRepository<Project> _projectRepository;
    private readonly IGitRepositoryService _gitRepositoryService;
    private readonly IGitProviderFactory _providerFactory;
    private readonly IGitAccountService _gitAccountService;
    private readonly IGitJobScheduler _gitJobScheduler;
    private readonly IGitBranchService _gitBranchService;
    private readonly ITaskCodeService _taskCodeService;
    private readonly IBacklogService _backlogService;
    private readonly IMergeRequestReviewService _mergeRequestReviewService;
    private readonly ICodeReviewWebhookHandler _codeReviewWebhookHandler;
    private readonly INotificationChannelService _notificationChannelService;
    private readonly IIssueSyncService _issueSyncService;
    private readonly ILogger<GitWebhookService> _logger;

    #endregion

    #region Ctor

    public GitWebhookService(IRepository<GitWebhookDelivery> deliveryRepository,
        IRepository<Project> projectRepository,
        IGitRepositoryService gitRepositoryService,
        IGitProviderFactory providerFactory,
        IGitAccountService gitAccountService,
        IGitJobScheduler gitJobScheduler,
        IGitBranchService gitBranchService,
        ITaskCodeService taskCodeService,
        IBacklogService backlogService,
        IMergeRequestReviewService mergeRequestReviewService,
        ICodeReviewWebhookHandler codeReviewWebhookHandler,
        INotificationChannelService notificationChannelService,
        IIssueSyncService issueSyncService,
        ILogger<GitWebhookService> logger)
    {
        _deliveryRepository = deliveryRepository;
        _projectRepository = projectRepository;
        _gitRepositoryService = gitRepositoryService;
        _providerFactory = providerFactory;
        _gitAccountService = gitAccountService;
        _gitJobScheduler = gitJobScheduler;
        _gitBranchService = gitBranchService;
        _taskCodeService = taskCodeService;
        _backlogService = backlogService;
        _mergeRequestReviewService = mergeRequestReviewService;
        _codeReviewWebhookHandler = codeReviewWebhookHandler;
        _notificationChannelService = notificationChannelService;
        _issueSyncService = issueSyncService;
        _logger = logger;
    }

    #endregion

    #region Methods

    public async Task<GitWebhookReceiveResult> ReceiveAsync(ProjectRepository repository, Func<string, string?> header, byte[] body, string? fallbackSecret = null)
    {
        if (body.Length == 0 || body.Length > MaxBodyBytes)
            return GitWebhookReceiveResult.Reject(400, "Invalid payload size.");

        var connection = repository.GitConnection;
        if (connection == null || connection.Deleted || !connection.Active)
            return GitWebhookReceiveResult.Reject(404, "Unknown repository.");

        var secret = string.IsNullOrWhiteSpace(repository.WebhookSecret) ? fallbackSecret : repository.WebhookSecret;
        if (string.IsNullOrWhiteSpace(secret))
        {
            _logger.LogWarning("Git webhook for repository {RepositoryId} rejected: no webhook secret is configured.", repository.Id);
            return GitWebhookReceiveResult.Reject(401, "Webhook secret not configured.");
        }

        var parser = _providerFactory.CreateWebhookParser(connection.ProviderType);
        if (!parser.Verify(header, body, secret))
        {
            _logger.LogWarning("Git webhook for repository {RepositoryId} rejected: signature check failed.", repository.Id);
            return GitWebhookReceiveResult.Reject(401, "Signature check failed.");
        }

        var eventName = Truncate(parser.EventName(header) ?? "unknown", 100);
        var deliveryId = parser.DeliveryId(header);
        if (string.IsNullOrWhiteSpace(deliveryId))
            deliveryId = "sha256:" + Convert.ToHexString(SHA256.HashData(body)).ToLowerInvariant();
        deliveryId = Truncate(deliveryId.Trim(), 100);

        if (await _deliveryRepository.Table.AnyAsync(x => x.ProjectRepositoryId == repository.Id && x.DeliveryId == deliveryId))
            return new GitWebhookReceiveResult { StatusCode = 200, Message = "Duplicate delivery ignored." };

        var delivery = new GitWebhookDelivery
        {
            ProjectRepositoryId = repository.Id,
            DeliveryId = deliveryId,
            EventName = eventName,
            Status = (int)GitWebhookDeliveryStatus.Queued,
            ReceivedOn = DateTime.UtcNow,
            Payload = Encoding.UTF8.GetString(body)
        };

        try
        {
            await _deliveryRepository.InsertAsync(delivery);
        }
        catch (DbUpdateException)
        {
            return new GitWebhookReceiveResult { StatusCode = 200, Message = "Duplicate delivery ignored." };
        }

        repository.WebhookLastDeliveryOn = DateTime.UtcNow;
        await _gitRepositoryService.UpdateRepositoryAsync(repository);

        await _gitJobScheduler.EnqueueWebhookAsync(delivery.Id);
        return new GitWebhookReceiveResult { StatusCode = 202, Message = "Accepted.", DeliveryId = delivery.Id };
    }

    public async Task ProcessAsync(int deliveryId)
    {
        var delivery = await _deliveryRepository.GetByIdAsync(deliveryId);
        if (delivery == null || delivery.Status == (int)GitWebhookDeliveryStatus.Processed || delivery.Status == (int)GitWebhookDeliveryStatus.Ignored)
            return;

        delivery.Attempts++;
        try
        {
            var repository = await _gitRepositoryService.GetContextAsync(delivery.ProjectRepositoryId);
            if (repository == null || string.IsNullOrWhiteSpace(delivery.Payload))
            {
                await FinishAsync(delivery, GitWebhookDeliveryStatus.Ignored, "The repository is no longer active.");
                return;
            }

            var parser = _providerFactory.CreateWebhookParser(repository.Connection.ProviderType);
            var webhookEvent = parser.Parse(delivery.EventName, delivery.Payload);

            if (!RepositoryMatches(repository, webhookEvent))
            {
                _logger.LogWarning("Git webhook delivery {DeliveryId} ignored: payload repository {Repository} does not match {Expected}.",
                    delivery.Id, webhookEvent.RepositoryPath ?? webhookEvent.RepositoryExternalId, repository.RepoRef);
                await FinishAsync(delivery, GitWebhookDeliveryStatus.Ignored, "The payload belongs to a different repository.");
                return;
            }

            await LearnRepositoryIdAsync(repository, webhookEvent);
            await DispatchAsync(repository, webhookEvent);
            await FinishAsync(delivery, GitWebhookDeliveryStatus.Processed, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Git webhook delivery {DeliveryId} failed.", delivery.Id);
            await FinishAsync(delivery, GitWebhookDeliveryStatus.Failed, ex.Message);
            throw;
        }
    }

    public async Task<int> PurgeAsync(int keepDays)
    {
        var before = DateTime.UtcNow.AddDays(-Math.Max(1, keepDays));
        return await _deliveryRepository.DeleteAsync(x => x.ReceivedOn < before);
    }

    #endregion

    #region Utilities

    private async Task DispatchAsync(GitRepositoryContext repository, GitWebhookEvent webhookEvent)
    {
        switch (webhookEvent.Kind)
        {
            case GitEventKind.Push when webhookEvent.Push != null:
                var pusher = await _gitAccountService.FindUserAsync(repository.Connection, webhookEvent.Push.PusherUserName);
                await _gitBranchService.HandlePushAsync(webhookEvent.Push, repository, pusher);
                break;

            case GitEventKind.MergeRequestNote:
                await _codeReviewWebhookHandler.HandleNoteAsync(repository, webhookEvent);
                break;

            case GitEventKind.MergeRequest when webhookEvent.MergeRequest != null:
                await HandleMergeRequestAsync(repository, webhookEvent);
                break;

            case GitEventKind.Issue:
            case GitEventKind.IssueComment:
                await _issueSyncService.HandleWebhookAsync(repository, webhookEvent);
                break;
        }
    }

    private async Task HandleMergeRequestAsync(GitRepositoryContext repository, GitWebhookEvent webhookEvent)
    {
        var mr = webhookEvent.MergeRequest!;
        if (mr.Iid <= 0)
            return;

        if (mr.Action == "open")
        {
            try
            {
                await _mergeRequestReviewService.AutoAssignReviewerAsync(repository, mr.Iid, mr.AuthorExternalId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Auto-assigning a reviewer for {Repository}{Ref} failed.", repository.RepoRef, repository.Capabilities.Ref(mr.Iid));
            }
        }

        var actorUserName = webhookEvent.Actor?.UserName;
        var actor = await _gitAccountService.FindUserAsync(repository.Connection, actorUserName);
        var label = repository.Capabilities.Label(mr.Iid);
        var taskIds = await _taskCodeService.ResolveTaskIdsForMergeRequestAsync(mr, repository);

        if (mr.State is "merged" or "closed" && mr.Action is "merge" or "close")
            await _backlogService.UpdateStatusOnMergeActionAsync(mr, actor, actorUserName, taskIds, repository.RepositoryId, label);
        else if (mr.State == "opened" && mr.IsReadyForReview)
            await _backlogService.MoveTasksForOpenedMergeRequestAsync(taskIds, mr.Iid, actor, actorUserName, repository.RepositoryId, label);

        foreach (var taskId in taskIds)
            await _taskCodeService.ClearCacheAsync(taskId);
        await _mergeRequestReviewService.ClearQueueCacheAsync(repository.RepositoryId);

        try
        {
            await _codeReviewWebhookHandler.HandleMergeRequestAsync(repository, webhookEvent);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Review notifications for {Repository}{Ref} failed.", repository.RepoRef, repository.Capabilities.Ref(mr.Iid));
        }

        var project = await _projectRepository.GetByIdAsync(repository.Repository.ProjectId);
        if (project != null && mr.Action is "open" or "reopen" or "merge" or "close")
        {
            try
            {
                var authorUserName = await _codeReviewWebhookHandler.AuthorUserNameAsync(repository, mr);
                var author = await _gitAccountService.FindUserAsync(repository.Connection, authorUserName);
                await _notificationChannelService.MergeRequestAsync(project.Id, $"{project.Name} / {repository.Name}", mr,
                    author?.Name ?? authorUserName ?? "someone", author, project.TeamLead ?? project.CodeReviewer);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Chat notification for {Repository}{Ref} failed.", repository.RepoRef, repository.Capabilities.Ref(mr.Iid));
            }
        }
    }

    private static bool RepositoryMatches(GitRepositoryContext repository, GitWebhookEvent webhookEvent)
    {
        if (webhookEvent.Kind is GitEventKind.Ping or GitEventKind.Unknown)
            return true;

        var known = repository.Repository.ExternalRepoId;
        if (!string.IsNullOrWhiteSpace(known) && !string.IsNullOrWhiteSpace(webhookEvent.RepositoryExternalId))
            return known == webhookEvent.RepositoryExternalId;

        if (!string.IsNullOrWhiteSpace(webhookEvent.RepositoryPath))
            return repository.Matches(webhookEvent.RepositoryPath) || repository.Matches(webhookEvent.RepositoryExternalId);

        return repository.Matches(webhookEvent.RepositoryExternalId);
    }

    private async Task LearnRepositoryIdAsync(GitRepositoryContext repository, GitWebhookEvent webhookEvent)
    {
        if (!string.IsNullOrWhiteSpace(repository.Repository.ExternalRepoId) || string.IsNullOrWhiteSpace(webhookEvent.RepositoryExternalId))
            return;

        repository.Repository.ExternalRepoId = Truncate(webhookEvent.RepositoryExternalId, 64);
        await _gitRepositoryService.UpdateRepositoryAsync(repository.Repository);
    }

    private async Task FinishAsync(GitWebhookDelivery delivery, GitWebhookDeliveryStatus status, string? error)
    {
        delivery.Status = (int)status;
        delivery.ProcessedOn = DateTime.UtcNow;
        delivery.Error = error == null ? null : Truncate(error, ErrorLength);
        if (status == GitWebhookDeliveryStatus.Processed || status == GitWebhookDeliveryStatus.Ignored)
            delivery.Payload = null;
        await _deliveryRepository.UpdateAsync(delivery);
    }

    private static string Truncate(string value, int length) => value.Length <= length ? value : value[..length];

    #endregion
}
