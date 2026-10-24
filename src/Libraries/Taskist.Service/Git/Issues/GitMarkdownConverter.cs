using System.Text.RegularExpressions;
using AngleSharp.Html.Parser;
using Markdig;
using Taskist.Service.Security;

namespace Taskist.Service.Git.Issues;

public class GitMarkdownConverter : IGitMarkdownConverter
{
    #region Fields

    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .DisableHtml()
        .UseAutoLinks()
        .UsePipeTables()
        .UseTaskLists()
        .UseEmphasisExtras()
        .UseSoftlineBreakAsHardlineBreak()
        .Build();

    private static readonly Regex InlineImage = new(@"!\[([^\]]*)\]\(([^)\s]+)[^)]*\)", RegexOptions.Compiled);

    private readonly IHtmlSanitizerService _htmlSanitizerService;

    #endregion

    #region Ctor

    public GitMarkdownConverter(IHtmlSanitizerService htmlSanitizerService)
    {
        _htmlSanitizerService = htmlSanitizerService;
    }

    #endregion

    #region Methods

    public string ToHtml(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        var withoutImages = InlineImage.Replace(markdown, m =>
        {
            var alt = string.IsNullOrWhiteSpace(m.Groups[1].Value) ? "image" : m.Groups[1].Value;
            return $"[{alt}]({m.Groups[2].Value})";
        });

        return _htmlSanitizerService.Sanitize(Markdown.ToHtml(withoutImages, Pipeline));
    }

    public string ToMarkdown(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var document = new HtmlParser().ParseDocument($"<body>{html}</body>");
        foreach (var image in document.QuerySelectorAll("img").ToList())
        {
            var alt = image.GetAttribute("alt");
            image.Replace(document.CreateTextNode($"[image{(string.IsNullOrWhiteSpace(alt) ? string.Empty : ": " + alt)} — attached in Taskist]"));
        }

        foreach (var link in document.QuerySelectorAll("a").ToList())
        {
            var href = link.GetAttribute("href");
            if (string.IsNullOrWhiteSpace(href) || !Uri.TryCreate(href, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
                link.Replace(document.CreateTextNode(link.TextContent));
        }

        foreach (var node in document.QuerySelectorAll("script,style,iframe,object,embed").ToList())
            node.Remove();

        var converter = new ReverseMarkdown.Converter(new ReverseMarkdown.Config
        {
            GithubFlavored = true,
            RemoveComments = true,
            SmartHrefHandling = true,
            UnknownTags = ReverseMarkdown.Config.UnknownTagsOption.Bypass
        });

        return Regex.Replace(converter.Convert(document.Body?.InnerHtml ?? string.Empty).Replace("\r\n", "\n"), @"\n{3,}", "\n\n").Trim();
    }

    #endregion
}
