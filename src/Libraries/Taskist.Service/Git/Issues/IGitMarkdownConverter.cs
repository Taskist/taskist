namespace Taskist.Service.Git.Issues;

public interface IGitMarkdownConverter
{
    string ToHtml(string? markdown);

    string ToMarkdown(string? html);
}
