using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Enums;

namespace Taskist.Service.Git.Providers.GitHub;

public class GitHubWebhookParser : IGitWebhookParser
{
    #region Methods

    public string? EventName(Func<string, string?> header) => header("X-GitHub-Event");

    public string? DeliveryId(Func<string, string?> header) => header("X-GitHub-Delivery");

    public bool Verify(Func<string, string?> header, byte[] body, string secret)
    {
        var signature = header("X-Hub-Signature-256");
        if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(secret) || !signature.StartsWith("sha256=", StringComparison.Ordinal))
            return false;

        byte[] supplied;
        try
        {
            supplied = Convert.FromHexString(signature["sha256=".Length..]);
        }
        catch (FormatException)
        {
            return false;
        }

        var expected = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), body);
        return CryptographicOperations.FixedTimeEquals(supplied, expected);
    }

    public GitWebhookEvent Parse(string eventName, string body)
    {
        var json = JObject.Parse(body);
        var result = new GitWebhookEvent
        {
            Action = json["action"]?.ToString()?.ToLowerInvariant(),
            RepositoryExternalId = json["repository"]?["id"]?.ToString(),
            RepositoryPath = json["repository"]?["full_name"]?.ToString(),
            Actor = json["sender"] is JObject sender ? GitHubProvider.Person(sender) : null
        };

        switch (eventName)
        {
            case "ping":
                result.Kind = GitEventKind.Ping;
                break;
            case "push":
                ParsePush(json, result);
                break;
            case "pull_request":
                ParsePullRequest(json, result);
                break;
            case "pull_request_review":
                ParseReview(json, result);
                break;
            case "issue_comment":
                ParseIssueComment(json, result);
                break;
            case "issues":
                if (json["issue"] is JObject issue)
                {
                    result.Kind = GitEventKind.Issue;
                    result.Issue = GitHubProvider.MapIssue(issue);
                }
                break;
        }

        return result;
    }

    #endregion

    #region Utilities

    private static void ParsePush(JObject json, GitWebhookEvent result)
    {
        var reference = json["ref"]?.ToString();
        result.Kind = GitEventKind.Push;
        result.Push = new GitPushInfo
        {
            BranchName = reference != null && reference.StartsWith("refs/heads/", StringComparison.Ordinal) ? reference["refs/heads/".Length..] : null,
            IsNewBranch = json["created"]?.Value<bool?>() == true,
            IsDeletedBranch = json["deleted"]?.Value<bool?>() == true,
            PusherUserName = json["sender"]?["login"]?.ToString() ?? json["pusher"]?["name"]?.ToString(),
            PusherName = json["pusher"]?["name"]?.ToString()
        };
    }

    private static void ParsePullRequest(JObject json, GitWebhookEvent result)
    {
        if (json["pull_request"] is not JObject pull)
            return;

        var merged = pull["merged"]?.Value<bool?>() == true;
        var action = result.Action switch
        {
            "opened" => "open",
            "reopened" => "reopen",
            "closed" => merged ? "merge" : "close",
            "ready_for_review" or "edited" or "synchronize" or "review_requested" or "converted_to_draft" => "update",
            _ => result.Action
        };

        var reviewers = new List<string>();
        if (result.Action == "review_requested" && json["requested_reviewer"]?["login"]?.ToString() is { Length: > 0 } reviewer)
            reviewers.Add(reviewer);
        if (result.Action == "opened")
            reviewers.AddRange((pull["requested_reviewers"] as JArray ?? []).Select(r => r["login"]?.ToString()).Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r!));

        result.Kind = GitEventKind.MergeRequest;
        result.Action = action;
        result.MergeRequest = Map(pull, action);
        result.MergeRequest.BecameReady = json["action"]?.ToString() == "ready_for_review";
        result.MergeRequest.NewReviewerUserNames = reviewers;
    }

    private static void ParseReview(JObject json, GitWebhookEvent result)
    {
        if (json["pull_request"] is not JObject pull || json["review"] is not JObject review || result.Action != "submitted")
            return;

        var state = review["state"]?.ToString()?.ToLowerInvariant();
        if (state == "approved")
        {
            result.Kind = GitEventKind.MergeRequest;
            result.Action = "approved";
            result.MergeRequest = Map(pull, "approved");
            return;
        }

        var body = review["body"]?.ToString();
        if (string.IsNullOrWhiteSpace(body))
            return;

        result.Kind = GitEventKind.MergeRequestNote;
        result.MergeRequest = Map(pull, "update");
        result.Note = new GitNoteInfo
        {
            MergeRequestIid = result.MergeRequest.Iid,
            MergeRequestTitle = result.MergeRequest.Title,
            MergeRequestUrl = result.MergeRequest.Url,
            MergeRequestAuthorUserName = result.MergeRequest.AuthorUserName,
            Body = body,
            Url = review["html_url"]?.ToString()
        };
    }

    private static void ParseIssueComment(JObject json, GitWebhookEvent result)
    {
        if (json["issue"] is not JObject issue || json["comment"] is not JObject comment)
            return;

        if (issue["pull_request"] != null)
        {
            result.Kind = GitEventKind.MergeRequestNote;
            if (result.Action != "created")
            {
                result.Kind = GitEventKind.Unknown;
                return;
            }

            result.MergeRequest = new GitMergeRequestInfo
            {
                Iid = issue["number"]?.Value<int?>() ?? 0,
                Title = issue["title"]?.ToString(),
                Url = issue["html_url"]?.ToString(),
                AuthorUserName = issue["user"]?["login"]?.ToString(),
                AuthorExternalId = issue["user"]?["id"]?.Value<long?>() ?? 0
            };
            result.Note = new GitNoteInfo
            {
                MergeRequestIid = result.MergeRequest.Iid,
                MergeRequestTitle = result.MergeRequest.Title,
                MergeRequestUrl = result.MergeRequest.Url,
                MergeRequestAuthorUserName = result.MergeRequest.AuthorUserName,
                Body = comment["body"]?.ToString(),
                Url = comment["html_url"]?.ToString()
            };
            return;
        }

        result.Kind = GitEventKind.IssueComment;
        result.Issue = GitHubProvider.MapIssue(issue);
        result.IssueComment = GitHubProvider.MapComment(comment);
    }

    private static GitMergeRequestInfo Map(JObject pull, string? action)
    {
        var merged = pull["merged"]?.Value<bool?>() == true || (pull["merged_at"] != null && pull["merged_at"]!.Type != JTokenType.Null);
        var open = string.Equals(pull["state"]?.ToString(), "open", StringComparison.OrdinalIgnoreCase);
        return new GitMergeRequestInfo
        {
            Iid = pull["number"]?.Value<int?>() ?? 0,
            Title = pull["title"]?.ToString(),
            Description = pull["body"]?.Type == JTokenType.Null ? null : pull["body"]?.ToString(),
            State = merged ? "merged" : open ? "opened" : "closed",
            Action = action,
            Draft = pull["draft"]?.Value<bool?>() == true,
            SourceBranch = pull["head"]?["ref"]?.ToString(),
            TargetBranch = pull["base"]?["ref"]?.ToString(),
            Url = pull["html_url"]?.ToString(),
            CreatedAt = pull["created_at"]?.ToObject<DateTime?>(),
            UpdatedAt = pull["updated_at"]?.ToString(),
            AuthorExternalId = pull["user"]?["id"]?.Value<long?>() ?? 0,
            AuthorUserName = pull["user"]?["login"]?.ToString(),
            Sha = pull["head"]?["sha"]?.ToString()
        };
    }

    #endregion
}
