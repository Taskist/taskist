using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Taskist.Core.Domain.Git;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Enums;
using Taskist.Data.Repository;
using Taskist.Service.Masters;
using Taskist.Service.WorkItems;

namespace Taskist.Service.Git.Issues;

public class IssueSyncService : IIssueSyncService
{
    #region Fields

    private const string TitleField = "title";
    private const string BodyField = "body";
    private const string ClosedField = "closed";
    private const string LabelsField = "labels";
    private const string AssigneeField = "assignee";
    private const string OutboundMarker = "wrote in Taskist:";
    private const int MaxTitleLength = 500;
    private const int MaxPollPages = 5;
    private const int MaxAutoExportPerRun = 20;
    private const int MaxLocalChecksPerRun = 200;

    private static readonly string[] Fields = [TitleField, BodyField, ClosedField, LabelsField, AssigneeField];

    private readonly IRepository<ExternalIssueLink> _linkRepository;
    private readonly IRepository<ExternalCommentLink> _commentLinkRepository;
    private readonly IRepository<Backlog> _backlogRepository;
    private readonly IRepository<BacklogComment> _backlogCommentRepository;
    private readonly IRepository<BacklogStatusLog> _statusLogRepository;
    private readonly IRepository<BacklogTag> _backlogTagRepository;
    private readonly IRepository<UserGitAccount> _accountRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IGitRepositoryService _gitRepositoryService;
    private readonly IGitMarkdownConverter _markdownConverter;
    private readonly IBacklogService _backlogService;
    private readonly ITagService _tagService;
    private readonly IStatusService _statusService;
    private readonly ITaskTypeService _taskTypeService;
    private readonly ISeverityService _severityService;
    private readonly IClosingRequirementService _closingRequirementService;
    private readonly ITaskCodeService _taskCodeService;
    private readonly ILogger<IssueSyncService> _logger;

    #endregion

    #region Ctor

    public IssueSyncService(IRepository<ExternalIssueLink> linkRepository,
        IRepository<ExternalCommentLink> commentLinkRepository,
        IRepository<Backlog> backlogRepository,
        IRepository<BacklogComment> backlogCommentRepository,
        IRepository<BacklogStatusLog> statusLogRepository,
        IRepository<BacklogTag> backlogTagRepository,
        IRepository<UserGitAccount> accountRepository,
        IRepository<User> userRepository,
        IGitRepositoryService gitRepositoryService,
        IGitMarkdownConverter markdownConverter,
        IBacklogService backlogService,
        ITagService tagService,
        IStatusService statusService,
        ITaskTypeService taskTypeService,
        ISeverityService severityService,
        IClosingRequirementService closingRequirementService,
        ITaskCodeService taskCodeService,
        ILogger<IssueSyncService> logger)
    {
        _linkRepository = linkRepository;
        _commentLinkRepository = commentLinkRepository;
        _backlogRepository = backlogRepository;
        _backlogCommentRepository = backlogCommentRepository;
        _statusLogRepository = statusLogRepository;
        _backlogTagRepository = backlogTagRepository;
        _accountRepository = accountRepository;
        _userRepository = userRepository;
        _gitRepositoryService = gitRepositoryService;
        _markdownConverter = markdownConverter;
        _backlogService = backlogService;
        _tagService = tagService;
        _statusService = statusService;
        _taskTypeService = taskTypeService;
        _severityService = severityService;
        _closingRequirementService = closingRequirementService;
        _taskCodeService = taskCodeService;
        _logger = logger;
    }

    #endregion

    #region Methods

    public async Task HandleWebhookAsync(GitRepositoryContext repository, GitWebhookEvent webhookEvent)
    {
        var mode = (IssueSyncMode)repository.Repository.IssueSyncMode;
        if (mode == IssueSyncMode.Off || webhookEvent.Issue == null || webhookEvent.Issue.Number <= 0)
            return;

        var issue = webhookEvent.Issue;
        var link = await FindLinkAsync(repository.RepositoryId, issue.Number);

        if (webhookEvent.Kind == GitEventKind.Issue)
        {
            if (webhookEvent.Action is "deleted" or "transferred")
            {
                if (link != null)
                {
                    link.SyncEnabled = false;
                    link.LastError = $"The issue was {webhookEvent.Action} on {repository.Capabilities.Name}.";
                    await _linkRepository.UpdateAsync(link);
                }
                return;
            }

            if (link == null)
            {
                if (CanImport(mode) && HasImportLabel(repository.Repository, issue) && !issue.Closed)
                    await ImportAsync(repository, issue);
                return;
            }

            if (link.SyncEnabled)
                await SyncLinkAsync(link, repository, issue);
            return;
        }

        if (webhookEvent.Kind == GitEventKind.IssueComment && link is { SyncEnabled: true } && webhookEvent.IssueComment != null
            && repository.Repository.IssueSyncComments && CanImport(mode) && webhookEvent.Action is "created" or "edited")
            await InboundCommentAsync(link, repository, webhookEvent.IssueComment);
    }

    public async Task PushAsync(int backlogId)
    {
        var links = await _linkRepository.Table.Where(x => x.BacklogId == backlogId && x.SyncEnabled).ToListAsync();
        foreach (var link in links)
        {
            var repository = await _gitRepositoryService.GetContextAsync(link.ProjectRepositoryId);
            if (repository == null || !CanExport((IssueSyncMode)repository.Repository.IssueSyncMode))
                continue;

            await SyncLinkAsync(link, repository, null);
            if (repository.Repository.IssueSyncComments)
                await OutboundCommentsAsync(link, repository);
        }
    }

    public async Task RunScheduledAsync()
    {
        foreach (var repositoryEntity in await _gitRepositoryService.GetIssueSyncRepositoriesAsync())
        {
            var repository = await _gitRepositoryService.GetContextAsync(repositoryEntity);
            if (repository == null || !repository.Provider.IsConfigured)
                continue;

            try
            {
                var mode = (IssueSyncMode)repositoryEntity.IssueSyncMode;
                if (CanImport(mode))
                    await PollAsync(repository);
                if (CanExport(mode))
                    await PushPendingAsync(repository);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Issue sync for repository {RepositoryId} failed.", repositoryEntity.Id);
            }
        }
    }

    public async Task<TaskIssueDto> GetTaskIssueAsync(int backlogId, bool canManage)
    {
        var result = new TaskIssueDto { CanManage = canManage };
        var task = await _backlogRepository.GetByIdAsync(backlogId);
        if (task == null)
            return result;

        var links = await _linkRepository.Table.Where(x => x.BacklogId == backlogId).OrderByDescending(x => x.SyncEnabled).ThenBy(x => x.Id).ToListAsync();
        foreach (var link in links)
        {
            var repository = await _gitRepositoryService.GetContextAsync(link.ProjectRepositoryId);
            result.Links.Add(new TaskIssueDto.Link
            {
                Id = link.Id,
                RepositoryId = link.ProjectRepositoryId,
                RepositoryName = repository?.Name ?? link.ProjectRepository?.RepoRef ?? "?",
                ProviderName = repository?.Capabilities.Name ?? "Git",
                Number = link.Number,
                Url = link.Url,
                RemoteState = link.RemoteState,
                SyncEnabled = link.SyncEnabled && repository != null,
                LastSyncedOn = link.LastSyncedOn,
                LastError = link.LastError,
                PublicRepository = repository?.Repository.IsPrivate == false,
                CommentsSynced = repository?.Repository.IssueSyncComments == true
            });
        }

        foreach (var repository in await _gitRepositoryService.GetProjectContextsAsync(task.ProjectId))
        {
            if (!CanExport((IssueSyncMode)repository.Repository.IssueSyncMode) || links.Any(l => l.ProjectRepositoryId == repository.RepositoryId && l.SyncEnabled))
                continue;

            result.PublishTargets.Add(new TaskIssueDto.Target
            {
                RepositoryId = repository.RepositoryId,
                Name = repository.Name,
                PublicRepository = repository.Repository.IsPrivate == false
            });
        }

        return result;
    }

    public async Task<bool> HasActiveLinkAsync(int backlogId)
    {
        return await _linkRepository.Table.AnyAsync(x => x.BacklogId == backlogId && x.SyncEnabled);
    }

    public async Task<IssueSyncResult> PublishAsync(int backlogId, int repositoryId, User user)
    {
        var task = await _backlogRepository.GetByIdAsync(backlogId);
        var repository = await _gitRepositoryService.GetContextAsync(repositoryId);
        if (task == null || task.Deleted || repository == null || repository.Repository.ProjectId != task.ProjectId)
            return IssueSyncResult.Fail("This repository is not linked to the task's project.");

        if (!CanExport((IssueSyncMode)repository.Repository.IssueSyncMode))
            return IssueSyncResult.Fail("Issue export is not enabled for this repository.");

        var existing = await _linkRepository.Table.FirstOrDefaultAsync(x => x.BacklogId == backlogId && x.ProjectRepositoryId == repositoryId);
        if (existing is { SyncEnabled: true })
            return IssueSyncResult.Fail($"This task is already linked to issue #{existing.Number}.");

        var link = await CreateRemoteIssueAsync(task, repository, existing);
        if (link == null)
            return IssueSyncResult.Fail(repository.Provider.LastError ?? "The issue was not created.");

        await AddSystemCommentAsync(task.Id, user.Id, $"Published as {repository.Capabilities.Name} issue #{link.Number} in {repository.Name} by {user.Name}");
        await _taskCodeService.ClearCacheAsync(task.Id);
        return IssueSyncResult.Ok($"Issue #{link.Number} created.");
    }

    public async Task<IssueSyncResult> SyncNowAsync(int backlogId, User user)
    {
        var links = await _linkRepository.Table.Where(x => x.BacklogId == backlogId && x.SyncEnabled).ToListAsync();
        if (links.Count == 0)
            return IssueSyncResult.Fail("This task is not linked to an issue.");

        var errors = new List<string>();
        foreach (var link in links)
        {
            var repository = await _gitRepositoryService.GetContextAsync(link.ProjectRepositoryId);
            if (repository == null)
                continue;

            await SyncLinkAsync(link, repository, null);
            var mode = (IssueSyncMode)repository.Repository.IssueSyncMode;
            if (repository.Repository.IssueSyncComments && CanExport(mode))
                await OutboundCommentsAsync(link, repository);
            if (repository.Repository.IssueSyncComments && CanImport(mode))
                await PullCommentsAsync(link, repository);
            if (!string.IsNullOrWhiteSpace(link.LastError))
                errors.Add(link.LastError);
        }

        return errors.Count == 0 ? IssueSyncResult.Ok("Synced.") : IssueSyncResult.Fail(string.Join(" ", errors));
    }

    public async Task<IssueSyncResult> UnlinkAsync(int backlogId, int linkId, User user)
    {
        var link = await _linkRepository.GetByIdAsync(linkId);
        if (link == null || link.BacklogId != backlogId)
            return IssueSyncResult.Fail("The issue link was not found.");

        link.SyncEnabled = false;
        await _linkRepository.UpdateAsync(link);
        await AddSystemCommentAsync(backlogId, user.Id, $"Stopped syncing with issue #{link.Number} ({user.Name}). The issue itself was not changed.");
        return IssueSyncResult.Ok("Sync stopped.");
    }

    #endregion

    #region Sync

    private async Task SyncLinkAsync(ExternalIssueLink link, GitRepositoryContext repository, GitIssueDto? remote)
    {
        var task = await _backlogRepository.GetByIdAsync(link.BacklogId);
        if (task == null || task.Deleted)
        {
            link.SyncEnabled = false;
            link.LastError = "The task was deleted.";
            await _linkRepository.UpdateAsync(link);
            return;
        }

        var provider = repository.Provider;
        remote ??= await provider.GetIssueAsync(repository.RepoRef, link.Number);
        if (remote == null)
        {
            link.LastError = Truncate(provider.LastError ?? "The issue could not be loaded.", 1000);
            await _linkRepository.UpdateAsync(link);
            return;
        }

        var mode = (IssueSyncMode)repository.Repository.IssueSyncMode;
        var snapshot = IssueSyncSnapshot.Parse(link.Snapshot);
        var local = await LocalValuesAsync(task, repository);
        var remoteValues = RemoteValues(remote, repository.Repository);

        var write = new GitIssueWriteDto();
        var pull = new List<string>();
        var writeNeeded = false;

        foreach (var field in Fields)
        {
            if (!snapshot.Fields.TryGetValue(field, out var baseline))
                continue;

            var localChanged = local[field].Hash != baseline.LocalHash;
            var remoteChanged = !string.Equals(remoteValues[field] ?? string.Empty, baseline.Remote ?? string.Empty, StringComparison.Ordinal);
            if (!localChanged && !remoteChanged)
                continue;

            var localWins = localChanged && (!remoteChanged || mode == IssueSyncMode.Export
                                             || (mode == IssueSyncMode.TwoWay && (task.ModifiedOn ?? task.CreatedOn).ToUniversalTime() >= remote.UpdatedAt.ToUniversalTime()));

            if (localChanged && remoteChanged)
                await AddSystemCommentAsync(task.Id, SyncUserId(repository, task),
                    $"Issue sync: {FieldLabel(field)} was changed both here and on {repository.Capabilities.Name} (issue #{link.Number}). Kept the {(localWins && CanExport(mode) ? "Taskist" : repository.Capabilities.Name)} version.");

            if (localWins && CanExport(mode) && local[field].Exportable)
            {
                writeNeeded |= ApplyToWrite(write, field, local[field], remote, repository.Repository);
            }
            else if (remoteChanged && CanImport(mode))
            {
                pull.Add(field);
            }
        }

        string? error = null;
        if (writeNeeded)
        {
            var updated = await provider.UpdateIssueAsync(repository.RepoRef, link.Number, write);
            if (updated.Success && updated.Data != null)
                remote = updated.Data;
            else
                error = Truncate(updated.Error ?? "The issue was not updated.", 1000);
        }

        if (pull.Count > 0)
            await ApplyToLocalAsync(task, repository, remote, pull);

        if (error != null)
        {
            link.LastError = error;
            await _linkRepository.UpdateAsync(link);
            return;
        }

        await SaveSnapshotAsync(link, task, repository, remote, null);
    }

    private async Task SaveSnapshotAsync(ExternalIssueLink link, Backlog task, GitRepositoryContext repository, GitIssueDto remote, string? error)
    {
        var finalLocal = await LocalValuesAsync(await _backlogRepository.GetByIdAsync(task.Id) ?? task, repository);
        var finalRemote = RemoteValues(remote, repository.Repository);
        var snapshot = new IssueSyncSnapshot();
        foreach (var field in Fields)
            snapshot.Fields[field] = new IssueSyncField { LocalHash = finalLocal[field].Hash, Remote = finalRemote[field] };

        link.Snapshot = snapshot.Serialize();
        link.RemoteState = remote.Closed ? "closed" : "open";
        link.RemoteUpdatedOn = remote.UpdatedAt;
        link.LastSyncedOn = DateTime.UtcNow;
        link.Url = remote.Url ?? link.Url;
        link.LastError = error;
        await _linkRepository.UpdateAsync(link);
    }

    private bool ApplyToWrite(GitIssueWriteDto write, string field, IssueLocalValue value, GitIssueDto remote, ProjectRepository repository)
    {
        switch (field)
        {
            case TitleField:
                write.Title = value.RemoteForm;
                return true;
            case BodyField:
                write.Body = value.RemoteForm ?? string.Empty;
                return true;
            case ClosedField:
                write.Closed = value.RemoteForm == "1";
                return true;
            case LabelsField:
                var labels = (value.RemoteForm ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
                var importLabel = repository.IssueImportLabel?.Trim();
                if (!string.IsNullOrWhiteSpace(importLabel) && remote.Labels.Any(l => string.Equals(l, importLabel, StringComparison.OrdinalIgnoreCase)))
                    labels.Add(importLabel);
                write.Labels = labels;
                return true;
            case AssigneeField:
                write.Assignees = string.IsNullOrEmpty(value.RemoteForm)
                    ? []
                    : [new GitPersonDto { Id = long.TryParse(value.RemoteForm.Split('|')[0], out var id) ? id : 0, UserName = value.RemoteForm.Split('|').ElementAtOrDefault(1) }];
                return true;
        }

        return false;
    }

    private async Task ApplyToLocalAsync(Backlog task, GitRepositoryContext repository, GitIssueDto remote, List<string> fields)
    {
        var actorId = SyncUserId(repository, task);
        var changed = false;
        var providerName = repository.Capabilities.Name;

        if (fields.Contains(TitleField) && !string.IsNullOrWhiteSpace(remote.Title))
        {
            var title = Truncate(remote.Title.Trim(), MaxTitleLength);
            if (title != task.Title)
            {
                await AddChangeCommentAsync(task.Id, actorId, "Title", task.Title, title, providerName);
                task.Title = title;
                changed = true;
            }
        }

        if (fields.Contains(BodyField))
        {
            task.Description = _markdownConverter.ToHtml(remote.Body);
            await AddSystemCommentAsync(task.Id, actorId, $"Description updated from {providerName} issue #{remote.Number}");
            changed = true;
        }

        if (fields.Contains(AssigneeField))
        {
            var assignee = await MapUserAsync(repository, remote.Assignees.FirstOrDefault());
            if (assignee != null && assignee.Id != task.AssigneeId)
            {
                await AddChangeCommentAsync(task.Id, actorId, "Assignee", task.Assignee?.Name, assignee.Name, providerName);
                task.AssigneeId = assignee.Id;
                changed = true;
            }
        }

        if (changed)
        {
            task.ModifiedOn = DateTime.Now;
            await _backlogRepository.UpdateAsync(task);
        }

        if (fields.Contains(LabelsField))
            await ApplyLabelsAsync(task, repository.Repository, remote);

        if (fields.Contains(ClosedField))
            await ApplyStateAsync(task, repository, remote, actorId);

        await _taskCodeService.ClearCacheAsync(task.Id);
    }

    private async Task ApplyLabelsAsync(Backlog task, ProjectRepository repository, GitIssueDto remote)
    {
        var importLabel = repository.IssueImportLabel?.Trim();
        var tagIds = new List<int>();
        foreach (var label in remote.Labels.Where(l => !string.Equals(l, importLabel, StringComparison.OrdinalIgnoreCase)).Take(30))
        {
            var name = Truncate(label.Trim(), 100);
            if (name.Length == 0)
                continue;

            var tag = await _tagService.GetOrCreateAsync(name);
            if (tag != null)
                tagIds.Add(tag.Id);
        }

        await _tagService.SetTaskTagsAsync(task.Id, tagIds.Distinct().ToArray());
    }

    private async Task ApplyStateAsync(Backlog task, GitRepositoryContext repository, GitIssueDto remote, int actorId)
    {
        var closedLocally = IsClosed(task);
        if (remote.Closed == closedLocally)
            return;

        var targetId = remote.Closed ? repository.Repository.IssueClosedStatusId : repository.Repository.IssueOpenStatusId;
        var providerName = repository.Capabilities.Name;
        if (!(targetId > 0))
        {
            await AddSystemCommentAsync(task.Id, actorId,
                $"Issue #{remote.Number} was {(remote.Closed ? "closed" : "reopened")} on {providerName}; no matching status is configured for this repository, so the task status was left as is.");
            return;
        }

        var status = await _statusService.GetByIdAsync(targetId.Value);
        if (status == null || status.Id == task.StatusId)
            return;

        if (remote.Closed)
        {
            var requirement = await _closingRequirementService.EvaluateAsync(task, status);
            var missing = new List<string>();
            if (requirement.RootCauseRequired && !requirement.HasRootCause) missing.Add("root cause");
            if (requirement.ResolutionRequired && !requirement.HasResolution) missing.Add("resolution");
            if (requirement.MrRequired && !requirement.HasMr) missing.Add("closing merge/pull request");
            if (missing.Count > 0)
            {
                await AddSystemCommentAsync(task.Id, actorId,
                    $"Issue #{remote.Number} was closed on {providerName}, but the task was not moved to {status.Name} because it still needs: {string.Join(", ", missing)}.");
                return;
            }
        }

        var oldName = task.Status?.Name;
        task.StatusId = status.Id;
        if (!remote.Closed && closedLocally)
            task.ReOpenCount++;
        task.ModifiedOn = DateTime.Now;
        await _backlogRepository.UpdateAsync(task);

        await _statusLogRepository.InsertAsync(new BacklogStatusLog
        {
            BacklogId = task.Id,
            StatusId = status.Id,
            CreatedById = actorId,
            CreatedOn = DateTime.Now
        });
        await AddSystemCommentAsync(task.Id, actorId, $"Status: {oldName} → {status.Name} via {providerName} issue #{remote.Number} {(remote.Closed ? "closed" : "reopened")}");
    }

    #endregion

    #region Import and export

    private async Task<ExternalIssueLink?> ImportAsync(GitRepositoryContext repository, GitIssueDto issue)
    {
        var settings = repository.Repository;
        var hourAgo = DateTime.UtcNow.AddHours(-1);
        var recent = await _linkRepository.Table.CountAsync(x => x.ProjectRepositoryId == settings.Id && x.CreatedOn >= hourAgo);
        var limit = settings.IssueImportLimitPerHour > 0 ? settings.IssueImportLimitPerHour : 50;
        if (recent >= limit)
        {
            _logger.LogWarning("Issue import for repository {RepositoryId} paused: {Count} issues linked in the last hour (limit {Limit}).", settings.Id, recent, limit);
            return null;
        }

        var project = settings.Project;
        var creatorId = settings.IssueSyncUserId ?? project?.TeamLeadId ?? project?.ProjectManagerId;
        if (!(creatorId > 0))
        {
            _logger.LogWarning("Issue import for repository {RepositoryId} skipped: no sync user is configured.", settings.Id);
            return null;
        }

        var taskTypeId = settings.IssueTaskTypeId ?? (await _taskTypeService.GetAllActiveAsync()).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefault();
        var severityId = settings.IssueSeverityId ?? (await _severityService.GetAllActiveAsync()).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefault();
        var statusId = settings.IssueOpenStatusId ?? (await _statusService.GetAllActiveByProjectAsync(settings.ProjectId))
            .Where(x => x.GroupId == (int)StatusGroup.New).OrderBy(x => x.Id).Select(x => (int?)x.Id).FirstOrDefault();
        if (taskTypeId == null || severityId == null || statusId == null)
        {
            _logger.LogWarning("Issue import for repository {RepositoryId} skipped: task type, severity or status is not configured.", settings.Id);
            return null;
        }

        var assignee = await MapUserAsync(repository, issue.Assignees.FirstOrDefault());
        var task = new Backlog
        {
            Title = Truncate(string.IsNullOrWhiteSpace(issue.Title) ? $"Issue #{issue.Number}" : issue.Title.Trim(), MaxTitleLength),
            Description = _markdownConverter.ToHtml(issue.Body),
            ProjectId = settings.ProjectId,
            TaskTypeId = taskTypeId.Value,
            SeverityId = severityId.Value,
            StatusId = statusId.Value,
            AssigneeId = assignee?.Id,
            CreatedById = creatorId.Value,
            CreatedOn = DateTime.Now,
            SourceId = (int)TaskSource.GitIssue
        };

        await _backlogService.InsertAsync(task, []);
        await ApplyLabelsAsync(task, settings, issue);

        var link = new ExternalIssueLink
        {
            BacklogId = task.Id,
            ProjectRepositoryId = settings.Id,
            ExternalId = issue.ExternalId,
            Number = issue.Number,
            Url = issue.Url,
            SyncEnabled = true,
            CreatedOn = DateTime.UtcNow
        };
        await _linkRepository.InsertAsync(link);

        var author = issue.Author?.UserName == null ? "someone" : "@" + issue.Author.UserName;
        await AddSystemCommentAsync(task.Id, creatorId.Value, $"Imported from {repository.Capabilities.Name} issue #{issue.Number} in {repository.Name} (opened by {author})");

        await SaveSnapshotAsync(link, task, repository, issue, null);

        if (settings.IssueSyncComments)
            await PullCommentsAsync(link, repository);

        return link;
    }

    private async Task<ExternalIssueLink?> CreateRemoteIssueAsync(Backlog task, GitRepositoryContext repository, ExternalIssueLink? existing)
    {
        var local = await LocalValuesAsync(task, repository);
        var write = new GitIssueWriteDto
        {
            Title = local[TitleField].RemoteForm,
            Body = local[BodyField].RemoteForm ?? string.Empty,
            Labels = (local[LabelsField].RemoteForm ?? string.Empty).Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList()
        };

        if (local[AssigneeField].Exportable && !string.IsNullOrEmpty(local[AssigneeField].RemoteForm))
            ApplyToWrite(write, AssigneeField, local[AssigneeField], new GitIssueDto(), repository.Repository);

        var created = await repository.Provider.CreateIssueAsync(repository.RepoRef, write);
        if (!created.Success || created.Data == null)
            return null;

        var issue = created.Data;
        if (IsClosed(task))
        {
            var closed = await repository.Provider.UpdateIssueAsync(repository.RepoRef, issue.Number, new GitIssueWriteDto { Closed = true });
            if (closed.Success && closed.Data != null)
                issue = closed.Data;
        }

        var link = existing ?? new ExternalIssueLink { BacklogId = task.Id, ProjectRepositoryId = repository.RepositoryId, CreatedOn = DateTime.UtcNow };
        link.ExternalId = issue.ExternalId;
        link.Number = issue.Number;
        link.Url = issue.Url;
        link.SyncEnabled = true;
        link.LastError = null;

        if (link.Id == 0)
            await _linkRepository.InsertAsync(link);

        await SaveSnapshotAsync(link, task, repository, issue, null);

        if (repository.Repository.IssueSyncComments)
            await OutboundCommentsAsync(link, repository);

        return link;
    }

    private async Task PollAsync(GitRepositoryContext repository)
    {
        var settings = repository.Repository;
        var since = settings.IssueLastPolledOn?.AddMinutes(-10) ?? settings.IssueSyncEnabledOn ?? DateTime.UtcNow.AddDays(-1);
        var startedOn = DateTime.UtcNow;

        for (var page = 1; page <= MaxPollPages; page++)
        {
            var issues = await repository.Provider.ListIssuesAsync(repository.RepoRef, since, null, page);
            if (issues == null)
            {
                _logger.LogWarning("Issue poll for repository {RepositoryId} failed: {Error}", settings.Id, repository.Provider.LastError);
                return;
            }

            foreach (var issue in issues)
            {
                var link = await FindLinkAsync(settings.Id, issue.Number);
                if (link == null)
                {
                    if (HasImportLabel(settings, issue) && !issue.Closed)
                        await ImportAsync(repository, issue);
                    continue;
                }

                if (link.SyncEnabled && (link.RemoteUpdatedOn == null || issue.UpdatedAt > link.RemoteUpdatedOn.Value.AddSeconds(1)))
                {
                    await SyncLinkAsync(link, repository, issue);
                    if (settings.IssueSyncComments)
                        await PullCommentsAsync(link, repository);
                }
            }

            if (issues.Count < 100)
                break;
        }

        settings.IssueLastPolledOn = startedOn;
        await _gitRepositoryService.UpdateRepositoryAsync(settings);
    }

    private async Task PushPendingAsync(GitRepositoryContext repository)
    {
        var settings = repository.Repository;
        var links = await _linkRepository.Table
            .Where(x => x.ProjectRepositoryId == settings.Id && x.SyncEnabled)
            .OrderBy(x => x.LastSyncedOn)
            .Take(MaxLocalChecksPerRun)
            .ToListAsync();

        foreach (var link in links)
        {
            var task = await _backlogRepository.GetByIdAsync(link.BacklogId);
            if (task == null)
                continue;

            var snapshot = IssueSyncSnapshot.Parse(link.Snapshot);
            var local = await LocalValuesAsync(task, repository);
            if (task.Deleted || Fields.Any(f => !snapshot.Fields.TryGetValue(f, out var baseline) || baseline.LocalHash != local[f].Hash))
                await SyncLinkAsync(link, repository, null);

            if (settings.IssueSyncComments)
                await OutboundCommentsAsync(link, repository);
        }

        var typeIds = (settings.IssueSyncTaskTypeIds ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => int.TryParse(x, out var id) ? id : 0)
            .Where(x => x > 0)
            .ToList();
        if (typeIds.Count == 0 || settings.IssueSyncEnabledOn == null)
            return;

        var enabledOn = settings.IssueSyncEnabledOn.Value.ToLocalTime();
        var candidates = await _backlogRepository.Table
            .Where(x => x.ProjectId == settings.ProjectId && !x.Deleted && typeIds.Contains(x.TaskTypeId) && x.CreatedOn >= enabledOn
                        && x.SourceId != (int)TaskSource.GitIssue
                        && !_linkRepository.Table.Any(l => l.BacklogId == x.Id && l.ProjectRepositoryId == settings.Id))
            .OrderBy(x => x.Id)
            .Take(MaxAutoExportPerRun)
            .ToListAsync();

        foreach (var task in candidates)
        {
            var link = await CreateRemoteIssueAsync(task, repository, null);
            if (link != null)
                await AddSystemCommentAsync(task.Id, SyncUserId(repository, task), $"Published automatically as {repository.Capabilities.Name} issue #{link.Number} in {repository.Name}");
            else
                _logger.LogWarning("Automatic issue export of task {TaskId} failed: {Error}", task.Id, repository.Provider.LastError);
        }
    }

    #endregion

    #region Comments

    private async Task InboundCommentAsync(ExternalIssueLink link, GitRepositoryContext repository, GitIssueCommentDto comment)
    {
        if (comment.System || string.IsNullOrWhiteSpace(comment.ExternalId) || string.IsNullOrWhiteSpace(comment.Body))
            return;

        var hash = Hash(comment.Body);
        var existing = await _commentLinkRepository.Table.FirstOrDefaultAsync(x => x.ExternalIssueLinkId == link.Id && x.ExternalCommentId == comment.ExternalId);
        if (existing != null)
        {
            if (!existing.Inbound || existing.ContentHash == hash)
                return;

            var stored = await _backlogCommentRepository.GetByIdAsync(existing.BacklogCommentId);
            if (stored != null)
            {
                stored.Comment = InboundCommentHtml(repository, comment);
                stored.ModifiedOn = DateTime.Now;
                await _backlogCommentRepository.UpdateAsync(stored);
            }

            existing.ContentHash = hash;
            existing.LastSyncedOn = DateTime.UtcNow;
            await _commentLinkRepository.UpdateAsync(existing);
            return;
        }

        if (IsOwnComment(repository, comment))
            return;

        var task = await _backlogRepository.GetByIdAsync(link.BacklogId);
        if (task == null || task.Deleted)
            return;

        var author = await MapUserAsync(repository, comment.Author);
        var backlogComment = new BacklogComment
        {
            BacklogId = task.Id,
            Comment = InboundCommentHtml(repository, comment),
            CreatedById = author?.Id ?? SyncUserId(repository, task),
            CreatedOn = DateTime.Now,
            SystemComment = false
        };
        await _backlogCommentRepository.InsertAsync(backlogComment);

        task.CommentCount = (task.CommentCount ?? 0) + 1;
        await _backlogRepository.UpdateAsync(task);

        await _commentLinkRepository.InsertAsync(new ExternalCommentLink
        {
            ExternalIssueLinkId = link.Id,
            BacklogCommentId = backlogComment.Id,
            ExternalCommentId = comment.ExternalId,
            Inbound = true,
            ContentHash = hash,
            LastSyncedOn = DateTime.UtcNow
        });
    }

    private async Task PullCommentsAsync(ExternalIssueLink link, GitRepositoryContext repository)
    {
        var comments = await repository.Provider.ListIssueCommentsAsync(repository.RepoRef, link.Number);
        foreach (var comment in comments ?? [])
            await InboundCommentAsync(link, repository, comment);
    }

    private async Task OutboundCommentsAsync(ExternalIssueLink link, GitRepositoryContext repository)
    {
        var linkedIds = await _commentLinkRepository.Table
            .Where(x => x.ExternalIssueLinkId == link.Id)
            .ToListAsync();

        var comments = await _backlogCommentRepository.Table
            .Where(x => x.BacklogId == link.BacklogId && x.PostToIssue && !x.SystemComment)
            .OrderBy(x => x.Id)
            .ToListAsync();

        foreach (var comment in comments)
        {
            var body = await OutboundCommentBodyAsync(comment);
            var hash = Hash(body);
            var existing = linkedIds.FirstOrDefault(x => x.BacklogCommentId == comment.Id);

            if (existing == null)
            {
                var created = await repository.Provider.CreateIssueCommentAsync(repository.RepoRef, link.Number, body);
                if (!created.Success || created.Data == null)
                {
                    link.LastError = Truncate(created.Error ?? "A comment could not be posted.", 1000);
                    await _linkRepository.UpdateAsync(link);
                    return;
                }

                await _commentLinkRepository.InsertAsync(new ExternalCommentLink
                {
                    ExternalIssueLinkId = link.Id,
                    BacklogCommentId = comment.Id,
                    ExternalCommentId = created.Data.ExternalId,
                    Inbound = false,
                    ContentHash = Hash(created.Data.Body ?? body),
                    LastSyncedOn = DateTime.UtcNow
                });
                continue;
            }

            if (existing.Inbound || existing.ContentHash == hash)
                continue;

            var updated = await repository.Provider.UpdateIssueCommentAsync(repository.RepoRef, link.Number, existing.ExternalCommentId, body);
            if (updated.Success)
            {
                existing.ContentHash = hash;
                existing.LastSyncedOn = DateTime.UtcNow;
                await _commentLinkRepository.UpdateAsync(existing);
            }
        }
    }

    private async Task<string> OutboundCommentBodyAsync(BacklogComment comment)
    {
        var author = comment.CreatedBy ?? await _userRepository.GetByIdAsync(comment.CreatedById);
        return $"**{author?.Name ?? "A Taskist user"}** {OutboundMarker}\n\n{comment.Comment?.Trim()}";
    }

    private static string InboundCommentHtml(GitRepositoryContext repository, GitIssueCommentDto comment)
    {
        var who = comment.Author?.DisplayName ?? "someone";
        var body = (comment.Body ?? string.Empty).Replace("\r\n", "\n").Trim();
        return Truncate($"{who} on {repository.Capabilities.Name}: {body}", 8000);
    }

    private static bool IsOwnComment(GitRepositoryContext repository, GitIssueCommentDto comment)
    {
        var service = repository.Connection.ServiceAccountUserName;
        if (!string.IsNullOrWhiteSpace(service) && string.Equals(comment.Author?.UserName, service, StringComparison.OrdinalIgnoreCase))
            return true;

        var firstLine = comment.Body?.Split('\n')[0] ?? string.Empty;
        return firstLine.StartsWith("**", StringComparison.Ordinal) && firstLine.EndsWith(OutboundMarker, StringComparison.Ordinal);
    }

    #endregion

    #region Values

    private async Task<Dictionary<string, IssueLocalValue>> LocalValuesAsync(Backlog task, GitRepositoryContext repository)
    {
        var importLabel = repository.Repository.IssueImportLabel?.Trim();
        var tags = await _backlogTagRepository.Table
            .Where(x => x.BacklogId == task.Id)
            .Select(x => x.Tag.Name)
            .ToListAsync();
        var labels = string.Join("\n", tags
            .Where(t => !string.IsNullOrWhiteSpace(t) && !string.Equals(t, importLabel, StringComparison.OrdinalIgnoreCase))
            .Select(t => t.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t, StringComparer.OrdinalIgnoreCase));

        IssueLocalValue assignee;
        if (task.AssigneeId > 0)
        {
            var account = await _accountRepository.Table.FirstOrDefaultAsync(x => x.UserId == task.AssigneeId && x.GitConnectionId == repository.Connection.Id);
            assignee = account == null
                ? new IssueLocalValue { Hash = Hash("user:" + task.AssigneeId), RemoteForm = null, Exportable = false }
                : new IssueLocalValue { Hash = Hash("user:" + task.AssigneeId), RemoteForm = $"{account.ExternalUserId}|{account.UserName}" };
        }
        else
        {
            assignee = new IssueLocalValue { Hash = Hash("user:none"), RemoteForm = string.Empty };
        }

        var title = Truncate(task.Title?.Trim() ?? string.Empty, 250);
        return new Dictionary<string, IssueLocalValue>
        {
            [TitleField] = new() { Hash = Hash(task.Title?.Trim() ?? string.Empty), RemoteForm = title },
            [BodyField] = new() { Hash = Hash(task.Description ?? string.Empty), RemoteForm = _markdownConverter.ToMarkdown(task.Description) },
            [ClosedField] = new() { Hash = Hash(IsClosed(task) ? "1" : "0"), RemoteForm = IsClosed(task) ? "1" : "0" },
            [LabelsField] = new() { Hash = Hash(labels), RemoteForm = labels },
            [AssigneeField] = assignee
        };
    }

    private static Dictionary<string, string?> RemoteValues(GitIssueDto issue, ProjectRepository repository)
    {
        var importLabel = repository.IssueImportLabel?.Trim();
        var labels = string.Join("\n", issue.Labels
            .Where(l => !string.IsNullOrWhiteSpace(l) && !string.Equals(l, importLabel, StringComparison.OrdinalIgnoreCase))
            .Select(l => l.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(l => l, StringComparer.OrdinalIgnoreCase));
        var assignee = issue.Assignees.FirstOrDefault();

        return new Dictionary<string, string?>
        {
            [TitleField] = issue.Title?.Trim(),
            [BodyField] = (issue.Body ?? string.Empty).Replace("\r\n", "\n").Trim(),
            [ClosedField] = issue.Closed ? "1" : "0",
            [LabelsField] = labels,
            [AssigneeField] = assignee == null ? string.Empty : assignee.Id.ToString()
        };
    }

    #endregion

    #region Utilities

    private static bool CanImport(IssueSyncMode mode) => mode is IssueSyncMode.Import or IssueSyncMode.TwoWay;

    private static bool CanExport(IssueSyncMode mode) => mode is IssueSyncMode.Export or IssueSyncMode.TwoWay;

    private static bool IsClosed(Backlog task)
    {
        var group = task.Status?.GroupId ?? 0;
        return group == (int)StatusGroup.Resolved || group == (int)StatusGroup.Archived;
    }

    private static bool HasImportLabel(ProjectRepository repository, GitIssueDto issue)
    {
        var label = repository.IssueImportLabel?.Trim();
        return !string.IsNullOrWhiteSpace(label) && issue.Labels.Any(l => string.Equals(l, label, StringComparison.OrdinalIgnoreCase));
    }

    private static string FieldLabel(string field) => field switch
    {
        TitleField => "the title",
        BodyField => "the description",
        ClosedField => "the open/closed state",
        LabelsField => "the labels",
        AssigneeField => "the assignee",
        _ => field
    };

    private static int SyncUserId(GitRepositoryContext repository, Backlog task)
    {
        return repository.Repository.IssueSyncUserId
               ?? repository.Repository.Project?.TeamLeadId
               ?? repository.Repository.Project?.ProjectManagerId
               ?? task.CreatedById;
    }

    private async Task<User?> MapUserAsync(GitRepositoryContext repository, GitPersonDto? person)
    {
        if (person == null || person.Id <= 0)
            return null;

        var externalId = person.Id.ToString();
        var userId = await _accountRepository.Table
            .Where(x => x.GitConnectionId == repository.Connection.Id && x.ExternalUserId == externalId)
            .Select(x => (int?)x.UserId)
            .FirstOrDefaultAsync();
        if (!(userId > 0))
            return null;

        var user = await _userRepository.GetByIdAsync(userId.Value);
        return user == null || user.Deleted ? null : user;
    }

    private async Task<ExternalIssueLink?> FindLinkAsync(int repositoryId, int number)
    {
        return await _linkRepository.Table.FirstOrDefaultAsync(x => x.ProjectRepositoryId == repositoryId && x.Number == number);
    }

    private async Task AddSystemCommentAsync(int backlogId, int userId, string text)
    {
        await _backlogCommentRepository.InsertAsync(new BacklogComment
        {
            BacklogId = backlogId,
            CreatedById = userId,
            CreatedOn = DateTime.UtcNow,
            Comment = Truncate(text, 4000),
            SystemComment = true
        });
    }

    private async Task AddChangeCommentAsync(int backlogId, int userId, string field, string? oldValue, string? newValue, string providerName)
    {
        await AddSystemCommentAsync(backlogId, userId,
            $"{field}: {Truncate(oldValue ?? "(none)", 300)} → {Truncate(newValue ?? "(none)", 300)} via {providerName}");
    }

    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static string Truncate(string value, int length) => value.Length <= length ? value : value[..length];

    #endregion
}
