using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Taskist.Web.Helpers.TagHelpers;

[HtmlTargetElement("wc-antiforgery-token", TagStructure = TagStructure.WithoutEndTag)]
public partial class AntiForgeryTokenTagHelper : TagHelper
{
    #region Properties

    protected IHtmlGenerator HtmlGenerator { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; }

    #endregion

    #region Ctor

    public AntiForgeryTokenTagHelper(IHtmlGenerator htmGenerator)
    {
        HtmlGenerator = htmGenerator;
    }

    #endregion

    #region Methods

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);

        ArgumentNullException.ThrowIfNull(output);

        output.SuppressOutput();

        var antiforgeryTag = HtmlGenerator.GenerateAntiforgery(ViewContext);
        if (antiforgeryTag != null)
            output.Content.SetHtmlContent(antiforgeryTag);

        return Task.CompletedTask;
    }

    #endregion
}