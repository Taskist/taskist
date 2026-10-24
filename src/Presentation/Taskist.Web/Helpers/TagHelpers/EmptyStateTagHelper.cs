using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
using System.Text.Encodings.Web;
using Taskist.Service.Security;

namespace Taskist.Web.Helpers.TagHelpers;

[HtmlTargetElement("wp-empty-state", TagStructure = TagStructure.NormalOrSelfClosing)]
public class EmptyStateTagHelper : TagHelper
{
    #region Fields

    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public EmptyStateTagHelper(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    #endregion

    #region Properties

    public string Icon { get; set; } = "fas fa-inbox";

    public string? Heading { get; set; }

    public string? Text { get; set; }

    public string Tone { get; set; } = "setup";

    public string? Size { get; set; }

    [HtmlAttributeName("action-text")]
    public string? ActionText { get; set; }

    [HtmlAttributeName("action-url")]
    public string? ActionUrl { get; set; }

    [HtmlAttributeName("action-icon")]
    public string ActionIcon { get; set; } = "fas fa-plus";

    [HtmlAttributeName("action-class")]
    public string? ActionClass { get; set; }

    [HtmlAttributeName("action-permission")]
    public string? ActionPermission { get; set; }

    [HtmlAttributeName("no-permission-text")]
    public string? NoPermissionText { get; set; }

    [HtmlAttributeName("secondary-text")]
    public string? SecondaryText { get; set; }

    [HtmlAttributeName("secondary-url")]
    public string? SecondaryUrl { get; set; }

    [HtmlAttributeName("secondary-class")]
    public string? SecondaryClass { get; set; }

    #endregion

    #region Methods

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var e = HtmlEncoder.Default;
        var extra = (await output.GetChildContentAsync()).GetContent();

        var canAct = string.IsNullOrEmpty(ActionPermission) || await _permissionService.AuthorizeAsync(ActionPermission);

        var html = new StringBuilder();
        html.Append("<div class=\"wp-empty__icon\" aria-hidden=\"true\"><i class=\"").Append(e.Encode(Icon)).Append("\"></i></div>");
        if (!string.IsNullOrWhiteSpace(Heading))
            html.Append("<h5 class=\"wp-empty__title\">").Append(e.Encode(Heading)).Append("</h5>");
        if (!string.IsNullOrWhiteSpace(Text))
            html.Append("<p class=\"wp-empty__text\">").Append(e.Encode(Text)).Append("</p>");
        if (!string.IsNullOrWhiteSpace(extra))
            html.Append("<div class=\"wp-empty__extra\">").Append(extra).Append("</div>");

        var actions = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(ActionText))
        {
            if (canAct)
            {
                actions.Append("<a class=\"btn btn-sm btn-primary wp-empty__action ").Append(e.Encode(ActionClass ?? string.Empty)).Append("\" href=\"")
                    .Append(e.Encode(string.IsNullOrWhiteSpace(ActionUrl) ? "#" : ActionUrl)).Append("\"><i class=\"")
                    .Append(e.Encode(ActionIcon)).Append(" me-1\"></i>").Append(e.Encode(ActionText)).Append("</a>");
            }
            else if (!string.IsNullOrWhiteSpace(NoPermissionText))
            {
                actions.Append("<span class=\"wp-empty__note\"><i class=\"fas fa-user-shield me-1\"></i>").Append(e.Encode(NoPermissionText)).Append("</span>");
            }
        }

        if (!string.IsNullOrWhiteSpace(SecondaryText))
        {
            actions.Append("<a class=\"wp-empty__link ").Append(e.Encode(SecondaryClass ?? string.Empty)).Append("\" href=\"")
                .Append(e.Encode(string.IsNullOrWhiteSpace(SecondaryUrl) ? "#" : SecondaryUrl)).Append("\">")
                .Append(e.Encode(SecondaryText)).Append("</a>");
        }

        if (actions.Length > 0)
            html.Append("<div class=\"wp-empty__actions\">").Append(actions).Append("</div>");

        var css = "wp-empty wp-empty--" + e.Encode(Tone);
        if (!string.IsNullOrWhiteSpace(Size))
            css += " wp-empty--" + e.Encode(Size);
        var existing = output.Attributes["class"]?.Value?.ToString();
        if (!string.IsNullOrWhiteSpace(existing))
            css += " " + existing;

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", css);
        output.Attributes.SetAttribute("role", "status");
        output.Content.SetHtmlContent(html.ToString());
    }

    #endregion
}
