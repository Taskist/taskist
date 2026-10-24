using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Enums;

namespace Taskist.Service.Git.Providers.GitLab;

public class GitLabWebhookParser : IGitWebhookParser
{
    #region Fields

    private const string EmptySha = "0000000000000000000000000000000000000000";

    #endregion

    #region Methods

    public string? EventName(Func<string, string?> header) => header("X-Gitlab-Event");

    public string? DeliveryId(Func<string, string?> header) =>
        header("X-Gitlab-Event-UUID") ?? header("Idempotency-Key") ?? header("X-Gitlab-Webhook-UUID");

    public bool Verify(Func<string, string?> header, byte[] body, string secret)
    {
        var supplied = header("X-Gitlab-Token");
        if (string.IsNullOrEmpty(supplied) || string.IsNullOrEmpty(secret))
            return false;

        return CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(supplied)),
            SHA256.HashData(Encoding.UTF8.GetBytes(secret)));
    }

    public GitWebhookEvent Parse(string eventName, string body)
    {
        var json = JObject.Parse(body);
        var result = new GitWebhookEvent
        {
            RepositoryExternalId = json["project"]?["id"]?.ToString() ?? json["project_id"]?.ToString(),
            RepositoryPath = json["project"]?["path_with_namespace"]?.ToString(),
            Actor = json["user"] is JObject user ? GitLabProvider.Person(user) : null
        };

        switch (json["object_kind"]?.ToString())
        {
            case "push":
                ParsePush(json, result);
                break;
            case "merge_request":
                ParseMergeRequest(json, result);
                break;
            case "note":
                ParseNote(json, result);
                break;
            case "issue":
                ParseIssue(json, result);
                break;
        }

        return result;
    }

    #endregion

    #region Utilities

    private static void ParsePush(JObject json, GitWebhookEvent result)
    {
        var reference = json["ref"]?.ToString();
        var before = json["before"]?.ToString();
        var after = json["after"]?.ToString();

        result.Kind = GitEventKind.Push;
        result.Push = new GitPushInfo
        {
            BranchName = reference != null && reference.StartsWith("refs/heads/", StringComparison.Ordinal) ? reference["refs/heads/".Length..] : null,
            IsNewBranch = before == EmptySha && after != EmptySha,
            IsDeletedBranch = after == EmptySha,
            PusherUserName = json["user_username"]?.ToString(),
            PusherName = json["user_name"]?.ToString()
        };
        result.Actor ??= new GitPersonDto { UserName = result.Push.PusherUserName, Name = result.Push.PusherName };
    }

    private static void ParseMergeRequest(JObject json, GitWebhookEvent result)
    {
        var attributes = json["object_attributes"];
        if (attributes == null)
            return;

        var action = attributes["action"]?.ToString()?.ToLowerInvariant();
        action = action switch
        {
            "approval" => "approved",
            "unapproval" => "unapproved",
            _ => action
        };

        var changes = json["changes"];
        bool WasDraft(string key) => changes?[key]?["previous"]?.Type == JTokenType.Boolean && (bool)changes[key]!["previous"]!;

        var newReviewers = action switch
        {
            "open" or "reopen" => UserNames(json["reviewers"]),
            "update" => UserNames(changes?["reviewers"]?["current"])
                .Except(UserNames(changes?["reviewers"]?["previous"]), StringComparer.OrdinalIgnoreCase)
                .ToList(),
            _ => []
        };

        result.Kind = GitEventKind.MergeRequest;
        result.Action = action;
        result.MergeRequest = new GitMergeRequestInfo
        {
            Iid = attributes["iid"]?.Value<int?>() ?? 0,
            Title = attributes["title"]?.ToString(),
            Description = attributes["description"]?.ToString(),
            State = attributes["state"]?.ToString()?.ToLowerInvariant(),
            Action = action,
            Draft = Bool(attributes["draft"]) || Bool(attributes["work_in_progress"]),
            BecameReady = action == "update" && (WasDraft("draft") || WasDraft("work_in_progress")),
            SourceBranch = attributes["source_branch"]?.ToString(),
            TargetBranch = attributes["target_branch"]?.ToString(),
            Url = attributes["url"]?.ToString(),
            CreatedAt = ParseDate(attributes["created_at"]?.ToString()),
            UpdatedAt = attributes["updated_at"]?.ToString(),
            AuthorExternalId = attributes["author_id"]?.Value<long?>() ?? 0,
            Sha = attributes["last_commit"]?["id"]?.ToString(),
            NewReviewerUserNames = newReviewers
        };
    }

    private static void ParseNote(JObject json, GitWebhookEvent result)
    {
        var attributes = json["object_attributes"];
        if (attributes == null)
            return;

        var noteableType = attributes["noteable_type"]?.ToString();
        if (string.Equals(noteableType, "MergeRequest", StringComparison.OrdinalIgnoreCase) && json["merge_request"] is JObject mr)
        {
            result.Kind = GitEventKind.MergeRequestNote;
            result.Note = new GitNoteInfo
            {
                MergeRequestIid = mr["iid"]?.Value<int?>() ?? 0,
                MergeRequestTitle = mr["title"]?.ToString(),
                MergeRequestUrl = mr["url"]?.ToString(),
                Body = attributes["note"]?.ToString(),
                Url = attributes["url"]?.ToString(),
                System = Bool(attributes["system"])
            };
            result.MergeRequest = new GitMergeRequestInfo { Iid = result.Note.MergeRequestIid, AuthorExternalId = mr["author_id"]?.Value<long?>() ?? 0 };
            return;
        }

        if (string.Equals(noteableType, "Issue", StringComparison.OrdinalIgnoreCase) && json["issue"] is JObject issue)
        {
            result.Kind = GitEventKind.IssueComment;
            result.Action = "created";
            result.Issue = GitLabProvider.MapIssue(issue);
            result.IssueComment = new GitIssueCommentDto
            {
                ExternalId = attributes["id"]?.ToString() ?? string.Empty,
                Body = attributes["note"]?.ToString(),
                Author = result.Actor,
                System = Bool(attributes["system"]),
                Url = attributes["url"]?.ToString(),
                CreatedAt = ParseDate(attributes["created_at"]?.ToString()) ?? DateTime.UtcNow,
                UpdatedAt = ParseDate(attributes["updated_at"]?.ToString()) ?? DateTime.UtcNow
            };
        }
    }

    private static void ParseIssue(JObject json, GitWebhookEvent result)
    {
        if (json["object_attributes"] is not JObject attributes)
            return;

        result.Kind = GitEventKind.Issue;
        result.Action = attributes["action"]?.ToString()?.ToLowerInvariant();

        var issue = GitLabProvider.MapIssue(attributes);
        if (json["labels"] is JArray labels)
            issue.Labels = labels.Select(l => l["title"]?.ToString()).Where(l => !string.IsNullOrWhiteSpace(l)).Select(l => l!).ToList();
        if (json["assignees"] is JArray assignees)
            issue.Assignees = assignees.OfType<JObject>().Select(GitLabProvider.Person).ToList();
        issue.CreatedAt = ParseDate(attributes["created_at"]?.ToString()) ?? issue.CreatedAt;
        issue.UpdatedAt = ParseDate(attributes["updated_at"]?.ToString()) ?? issue.UpdatedAt;
        result.Issue = issue;
    }

    private static bool Bool(JToken? token) => token?.Type == JTokenType.Boolean && token.Value<bool>();

    private static List<string> UserNames(JToken? token)
    {
        if (token is not JArray array)
            return [];

        return array
            .Select(x => x?["username"]?.ToString())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var text = value.Trim();
        if (text.EndsWith(" UTC", StringComparison.OrdinalIgnoreCase))
            text = text[..^4] + "Z";

        return DateTime.TryParse(text, System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal, out var parsed)
            ? parsed
            : null;
    }

    #endregion
}
