using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Taskist.Core.Dtos.Notification;
using Taskist.Core.Enums;

namespace Taskist.Service.Messages;

public static class ChatPayloadBuilder
{
    #region Fields

    private const string OpenText = "Open in Taskist";

    #endregion

    #region Methods

    public static IList<string> Build(NotificationChannelType type, ChatMessage message)
    {
        return type switch
        {
            NotificationChannelType.TeamsFlowPerson => message.Recipients
                .Where(r => !string.IsNullOrWhiteSpace(r.Email))
                .Select(r => Serialize(new JObject { ["email"] = r.Email, ["message"] = TeamsFlowHtml(message, r) }))
                .ToList(),
            NotificationChannelType.TeamsChannel => [Serialize(TeamsCard(message))],
            NotificationChannelType.SlackChannel => [Serialize(Slack(message))],
            NotificationChannelType.GoogleChat => [Serialize(new JObject { ["text"] = GoogleChatText(message) })],
            NotificationChannelType.Discord => [Serialize(Discord(message))],
            NotificationChannelType.Mattermost => [Serialize(new JObject { ["text"] = MarkdownText(message) })],
            _ => [Serialize(Generic(message))]
        };
    }

    #endregion

    #region Utilities

    private static string Serialize(JToken token) => token.ToString(Formatting.None);

    private static string ForLine(ChatMessage message)
    {
        var names = message.Recipients.Select(r => r.Name).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToList();
        return names.Count == 0 ? string.Empty : "For: " + string.Join(", ", names);
    }

    private static string TeamsFlowHtml(ChatMessage message, ChatRecipient recipient)
    {
        var html = new StringBuilder();
        html.Append($"<p><strong>Hello {WebUtility.HtmlEncode(recipient.Name)}</strong>,</p>");
        html.Append($"<p><strong>{WebUtility.HtmlEncode(message.Title)}</strong></p>");
        if (message.Lines.Count > 0)
            html.Append("<ul>").Append(string.Concat(message.Lines.Select(l => $"<li>{WebUtility.HtmlEncode(l)}</li>"))).Append("</ul>");
        if (!string.IsNullOrWhiteSpace(message.Url))
            html.Append($"<p><a href=\"{WebUtility.HtmlEncode(message.Url)}\">{OpenText}</a></p>");
        return html.ToString();
    }

    private static JObject TeamsCard(ChatMessage message)
    {
        var body = new JArray
        {
            new JObject { ["type"] = "TextBlock", ["text"] = message.Title, ["weight"] = "Bolder", ["wrap"] = true }
        };
        foreach (var line in message.Lines)
            body.Add(new JObject { ["type"] = "TextBlock", ["text"] = line, ["wrap"] = true, ["spacing"] = "Small" });
        var forLine = ForLine(message);
        if (forLine.Length > 0)
            body.Add(new JObject { ["type"] = "TextBlock", ["text"] = forLine, ["wrap"] = true, ["isSubtle"] = true });

        var card = new JObject
        {
            ["$schema"] = "http://adaptivecards.io/schemas/adaptive-card.json",
            ["type"] = "AdaptiveCard",
            ["version"] = "1.4",
            ["body"] = body
        };
        if (!string.IsNullOrWhiteSpace(message.Url))
            card["actions"] = new JArray { new JObject { ["type"] = "Action.OpenUrl", ["title"] = OpenText, ["url"] = message.Url } };

        return new JObject
        {
            ["type"] = "message",
            ["attachments"] = new JArray
            {
                new JObject { ["contentType"] = "application/vnd.microsoft.card.adaptive", ["content"] = card }
            }
        };
    }

    private static string SlackEscape(string value) => value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

    private static JObject Slack(ChatMessage message)
    {
        var text = new StringBuilder($"*{SlackEscape(message.Title)}*");
        foreach (var line in message.Lines)
            text.Append('\n').Append(SlackEscape(line));
        var forLine = ForLine(message);
        if (forLine.Length > 0)
            text.Append('\n').Append('_').Append(SlackEscape(forLine)).Append('_');
        if (!string.IsNullOrWhiteSpace(message.Url))
            text.Append('\n').Append($"<{message.Url}|{OpenText}>");

        return new JObject
        {
            ["text"] = SlackEscape(message.Title),
            ["blocks"] = new JArray
            {
                new JObject { ["type"] = "section", ["text"] = new JObject { ["type"] = "mrkdwn", ["text"] = Truncate(text.ToString(), 2900) } }
            }
        };
    }

    private static string GoogleChatText(ChatMessage message)
    {
        static string Clean(string value) => value.Replace("<", "‹").Replace(">", "›");

        var text = new StringBuilder($"*{Clean(message.Title)}*");
        foreach (var line in message.Lines)
            text.Append('\n').Append(Clean(line));
        var forLine = ForLine(message);
        if (forLine.Length > 0)
            text.Append('\n').Append('_').Append(Clean(forLine)).Append('_');
        if (!string.IsNullOrWhiteSpace(message.Url))
            text.Append('\n').Append($"<{message.Url}|{OpenText}>");
        return Truncate(text.ToString(), 4000);
    }

    private static string MarkdownEscape(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            if ("\\`*_{}[]()#+-.!|<>~@".IndexOf(c) >= 0)
                builder.Append('\\');
            builder.Append(c);
        }
        return builder.ToString();
    }

    private static string MarkdownText(ChatMessage message)
    {
        var text = new StringBuilder($"**{MarkdownEscape(message.Title)}**");
        foreach (var line in message.Lines)
            text.Append('\n').Append(MarkdownEscape(line));
        var forLine = ForLine(message);
        if (forLine.Length > 0)
            text.Append('\n').Append('_').Append(MarkdownEscape(forLine)).Append('_');
        if (!string.IsNullOrWhiteSpace(message.Url))
            text.Append('\n').Append($"[{OpenText}]({message.Url})");
        return Truncate(text.ToString(), 4000);
    }

    private static JObject Discord(ChatMessage message)
    {
        var description = string.Join("\n", message.Lines.Select(MarkdownEscape));
        var forLine = ForLine(message);
        if (forLine.Length > 0)
            description += (description.Length > 0 ? "\n" : string.Empty) + "_" + MarkdownEscape(forLine) + "_";

        var embed = new JObject
        {
            ["title"] = Truncate(message.Title, 256),
            ["description"] = Truncate(description, 4000)
        };
        if (!string.IsNullOrWhiteSpace(message.Url))
            embed["url"] = message.Url;

        return new JObject
        {
            ["embeds"] = new JArray { embed },
            ["allowed_mentions"] = new JObject { ["parse"] = new JArray() }
        };
    }

    private static JObject Generic(ChatMessage message)
    {
        return new JObject
        {
            ["event"] = message.Event.ToString(),
            ["project"] = message.ProjectName,
            ["title"] = message.Title,
            ["lines"] = new JArray(message.Lines),
            ["url"] = message.Url,
            ["recipients"] = new JArray(message.Recipients.Select(r => new JObject { ["name"] = r.Name, ["email"] = r.Email })),
            ["sentAt"] = DateTime.UtcNow.ToString("o")
        };
    }

    private static string Truncate(string value, int length) => value.Length <= length ? value : value[..(length - 1)] + "…";

    #endregion
}
