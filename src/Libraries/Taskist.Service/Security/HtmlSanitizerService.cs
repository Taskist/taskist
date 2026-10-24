using Ganss.Xss;

namespace Taskist.Service.Security;

public class HtmlSanitizerService : IHtmlSanitizerService
{
    #region Fields

    private static readonly HtmlSanitizer Sanitizer = Create();

    #endregion

    #region Methods

    public string Sanitize(string? html)
    {
        return string.IsNullOrWhiteSpace(html) ? string.Empty : Sanitizer.Sanitize(html);
    }

    #endregion

    #region Utilities

    private static HtmlSanitizer Create()
    {
        var sanitizer = new HtmlSanitizer();

        sanitizer.AllowedSchemes.Clear();
        sanitizer.AllowedSchemes.Add("https");
        sanitizer.AllowedSchemes.Add("http");
        sanitizer.AllowedSchemes.Add("mailto");

        sanitizer.AllowedTags.Remove("img");
        sanitizer.AllowedTags.Remove("form");
        sanitizer.AllowedTags.Remove("input");
        sanitizer.AllowedTags.Remove("button");
        sanitizer.AllowedTags.Remove("textarea");
        sanitizer.AllowedTags.Remove("select");
        sanitizer.AllowedTags.Remove("style");

        sanitizer.AllowedAttributes.Remove("style");
        sanitizer.AllowedAttributes.Remove("id");
        sanitizer.AllowedAttributes.Remove("name");
        sanitizer.AllowedCssProperties.Clear();
        sanitizer.AllowDataAttributes = false;

        sanitizer.PostProcessNode += (_, e) =>
        {
            if (e.Node is AngleSharp.Dom.IElement { TagName: "A" } link)
            {
                link.SetAttribute("rel", "noopener noreferrer nofollow");
                link.SetAttribute("target", "_blank");
            }
        };

        return sanitizer;
    }

    #endregion
}
