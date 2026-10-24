using Microsoft.EntityFrameworkCore;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Dtos.Notification;
using Taskist.Core.Enums;
using Taskist.Data.Repository;
using Taskist.Service.Git;
using Taskist.Service.Messages;

namespace Taskist.Service.WorkItems;

public class CodeReviewNotificationService : ICodeReviewNotificationService
{
    #region Fields

    private const int ActionDedupeHours = 1;
    private const int ReviewerDedupeHours = 24 * 30;
    private const int CommentPreviewLength = 240;

    protected readonly IUserNotificationService _userNotificationService;
    protected readonly IRepository<User> _userRepository;
    protected readonly IRepository<Project> _projectRepository;
    protected readonly IGitRepositoryService _gitRepositoryService;
    protected readonly IGitAccountService _gitAccountService;

    #endregion

    #region Ctor

    public CodeReviewNotificationService(IUserNotificationService userNotificationService,
        IRepository<User> userRepository,
        IRepository<Project> projectRepository,
        IGitRepositoryService gitRepositoryService,
        IGitAccountService gitAccountService)
    {
        _userNotificationService = userNotificationService;
        _userRepository = userRepository;
        _projectRepository = projectRepository;
        _gitRepositoryService = gitRepositoryService;
        _gitAccountService = gitAccountService;
    }

    #endregion

    #region Methods

    public async Task ReviewerAssignedAsync(int projectId, MergeRequestRefDto mergeRequest, IEnumerable<string?> reviewerUserNames, string? actorUserName)
    {
        var names = reviewerUserNames
            .Where(x => !string.IsNullOrWhiteSpace(x) && !Same(x, actorUserName) && !Same(x, mergeRequest.AuthorUserName))
            .Select(x => x!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (names.Count == 0)
            return;

        var project = await GetProjectAsync(projectId);
        foreach (var name in names)
        {
            var userId = await FindUserIdAsync(mergeRequest, name);
            if (userId == 0)
                continue;

            await SafeNotifyAsync(new UserNotificationRequest
            {
                UserIds = [userId],
                Type = NotificationType.ReviewerAssigned,
                Title = $"Review requested: {mergeRequest.RefPrefix}{mergeRequest.Iid} {mergeRequest.Title}",
                Message = $"{project?.Name} · by {await AuthorDisplayAsync(mergeRequest)}",
                Url = mergeRequest.WebUrl,
                DedupeKey = Key("reviewer", projectId, mergeRequest, name),
                DedupeHours = ReviewerDedupeHours,
                ChatProjectId = projectId
            });
        }
    }

    public async Task ApprovedAsync(int projectId, MergeRequestRefDto mergeRequest, string? approverUserName, string approverName)
    {
        if (Same(approverUserName, mergeRequest.AuthorUserName))
            return;

        var authorId = await FindUserIdAsync(mergeRequest, mergeRequest.AuthorUserName);
        if (authorId == 0)
            return;

        var project = await GetProjectAsync(projectId);
        await SafeNotifyAsync(new UserNotificationRequest
        {
            UserIds = [authorId],
            Type = NotificationType.MergeRequestApproved,
            Title = $"Approved: {mergeRequest.RefPrefix}{mergeRequest.Iid} {mergeRequest.Title}",
            Message = $"{project?.Name} · approved by {approverName}",
            Url = mergeRequest.WebUrl,
            DedupeKey = Key("approved", projectId, mergeRequest, approverUserName),
            DedupeHours = ActionDedupeHours,
            ChatProjectId = projectId
        });
    }

    public async Task ChangesRequestedAsync(int projectId, MergeRequestRefDto mergeRequest, string? reviewerUserName, string reviewerName, string notes)
    {
        if (Same(reviewerUserName, mergeRequest.AuthorUserName))
            return;

        var authorId = await FindUserIdAsync(mergeRequest, mergeRequest.AuthorUserName);
        if (authorId == 0)
            return;

        var project = await GetProjectAsync(projectId);
        await SafeNotifyAsync(new UserNotificationRequest
        {
            UserIds = [authorId],
            Type = NotificationType.ChangesRequested,
            Title = $"Changes requested: {mergeRequest.RefPrefix}{mergeRequest.Iid} {mergeRequest.Title}",
            Message = $"{project?.Name} · {reviewerName}: {Preview(notes)}",
            Url = mergeRequest.WebUrl,
            Email = true,
            ChatProjectId = projectId
        });
    }

    public async Task RejectedAsync(int projectId, MergeRequestRefDto mergeRequest, string? actorUserName, string actorName, string reason)
    {
        if (Same(actorUserName, mergeRequest.AuthorUserName))
            return;

        var authorId = await FindUserIdAsync(mergeRequest, mergeRequest.AuthorUserName);
        if (authorId == 0)
            return;

        var project = await GetProjectAsync(projectId);
        await SafeNotifyAsync(new UserNotificationRequest
        {
            UserIds = [authorId],
            Type = NotificationType.MergeRequestRejected,
            Title = $"Rejected: {mergeRequest.RefPrefix}{mergeRequest.Iid} {mergeRequest.Title}",
            Message = $"{project?.Name} · {actorName}: {Preview(reason)}",
            Url = mergeRequest.WebUrl,
            DedupeKey = Key("rejected", projectId, mergeRequest, null),
            DedupeHours = ActionDedupeHours,
            Email = true,
            ChatProjectId = projectId
        });
    }

    public async Task ClosedAsync(int projectId, MergeRequestRefDto mergeRequest, string? actorUserName, string? actorName)
    {
        if (Same(actorUserName, mergeRequest.AuthorUserName))
            return;

        var authorId = await FindUserIdAsync(mergeRequest, mergeRequest.AuthorUserName);
        if (authorId == 0)
            return;

        if (await _userNotificationService.ExistsAsync(authorId, Key("rejected", projectId, mergeRequest, null), ActionDedupeHours))
            return;

        var project = await GetProjectAsync(projectId);
        await SafeNotifyAsync(new UserNotificationRequest
        {
            UserIds = [authorId],
            Type = NotificationType.MergeRequestClosed,
            Title = $"Closed: {mergeRequest.RefPrefix}{mergeRequest.Iid} {mergeRequest.Title}",
            Message = $"{project?.Name} · closed by {actorName ?? actorUserName ?? "someone"} without merging",
            Url = mergeRequest.WebUrl,
            DedupeKey = Key("closed", projectId, mergeRequest, null),
            DedupeHours = ActionDedupeHours
        });
    }

    public async Task CommentedAsync(int projectId, MergeRequestRefDto mergeRequest, string? commenterUserName, string? commenterName, string? note, string? noteUrl)
    {
        if (string.IsNullOrWhiteSpace(note) || Same(commenterUserName, mergeRequest.AuthorUserName))
            return;

        var authorId = await FindUserIdAsync(mergeRequest, mergeRequest.AuthorUserName);
        if (authorId == 0)
            return;

        var project = await GetProjectAsync(projectId);
        await SafeNotifyAsync(new UserNotificationRequest
        {
            UserIds = [authorId],
            Type = NotificationType.MergeRequestComment,
            Title = $"New comment on {mergeRequest.RefPrefix}{mergeRequest.Iid} {mergeRequest.Title}",
            Message = $"{commenterName ?? commenterUserName}: {Preview(note)}",
            Url = noteUrl ?? mergeRequest.WebUrl,
            DedupeKey = string.IsNullOrWhiteSpace(noteUrl) ? null : Truncate("note:" + noteUrl, 200),
            DedupeHours = ActionDedupeHours,
            ChatProjectId = null
        });
    }

    public async Task<bool> ConflictAsync(int projectId, string projectName, MergeRequestRefDto mergeRequest)
    {
        var authorId = await FindUserIdAsync(mergeRequest, mergeRequest.AuthorUserName);
        if (authorId == 0)
            return false;

        var project = await GetProjectAsync(projectId);
        var sent = await SafeNotifyAsync(new UserNotificationRequest
        {
            UserIds = [authorId],
            Type = NotificationType.MergeRequestConflict,
            Title = $"Merge conflict: {mergeRequest.RefPrefix}{mergeRequest.Iid} {mergeRequest.Title}",
            Message = $"{projectName} · the merge request no longer merges cleanly. Rebase or resolve the conflicts.",
            Url = mergeRequest.WebUrl,
            DedupeKey = Key("conflict", projectId, mergeRequest, mergeRequest.Sha ?? "head"),
            Email = true,
            ChatProjectId = projectId
        });

        return sent > 0;
    }

    #endregion

    #region Utilities

    private async Task<int> SafeNotifyAsync(UserNotificationRequest request)
    {
        try
        {
            return await _userNotificationService.NotifyAsync(request);
        }
        catch (Exception)
        {
            return 0;
        }
    }

    private async Task<Project?> GetProjectAsync(int projectId)
    {
        return projectId > 0 ? await _projectRepository.Table.FirstOrDefaultAsync(x => x.Id == projectId) : null;
    }

    private async Task<User?> FindUserAsync(MergeRequestRefDto mergeRequest, string? gitUserName)
    {
        var name = gitUserName?.Trim();
        if (string.IsNullOrEmpty(name))
            return null;

        var connection = mergeRequest.ConnectionId > 0
            ? await _gitRepositoryService.GetConnectionByIdAsync(mergeRequest.ConnectionId)
            : await _gitRepositoryService.GetDefaultConnectionAsync();
        if (connection != null)
            return await _gitAccountService.FindUserAsync(connection, name);

        return await _userRepository.Table.FirstOrDefaultAsync(x => !x.Deleted && x.GitUserName == name);
    }

    private async Task<int> FindUserIdAsync(MergeRequestRefDto mergeRequest, string? gitUserName)
    {
        return (await FindUserAsync(mergeRequest, gitUserName))?.Id ?? 0;
    }

    private async Task<string> AuthorDisplayAsync(MergeRequestRefDto mergeRequest)
    {
        var name = mergeRequest.AuthorUserName?.Trim();
        if (string.IsNullOrEmpty(name))
            return "unknown";

        return (await FindUserAsync(mergeRequest, name))?.Name ?? name;
    }

    private static string Key(string kind, int projectId, MergeRequestRefDto mergeRequest, string? suffix)
    {
        var key = mergeRequest.RepositoryId > 0
            ? $"{kind}:{projectId}:{mergeRequest.RepositoryId}:{mergeRequest.Iid}"
            : $"{kind}:{projectId}:{mergeRequest.Iid}";
        return string.IsNullOrWhiteSpace(suffix) ? key : Truncate($"{key}:{suffix.Trim().ToLowerInvariant()}", 200);
    }

    private static bool Same(string? a, string? b) =>
        !string.IsNullOrWhiteSpace(a) && !string.IsNullOrWhiteSpace(b) && string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);

    private static string Preview(string? text)
    {
        var value = (text ?? string.Empty).Trim();
        return value.Length <= CommentPreviewLength ? value : value[..CommentPreviewLength] + "…";
    }

    private static string Truncate(string value, int length) => value.Length <= length ? value : value[..length];

    #endregion
}
