using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Net;
using Taskist.Core.Common;
using Taskist.Service.Localization;
using Taskist.Web.Helpers.Attributes;

namespace Taskist.Web.Helpers.TagHelpers;

[HtmlTargetElement("wc-label", Attributes = FOR_ATTRIBUTE_NAME, TagStructure = TagStructure.WithoutEndTag)]
public class LabelTagHelper : TagHelper
{
    #region Constants

    protected const string FOR_ATTRIBUTE_NAME = "asp-for";
    protected const string DISPLAY_HINT_ATTRIBUTE_NAME = "asp-display-hint";

    #endregion

    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly IWorkContext _workContext;

    #endregion

    #region Properties

    protected IHtmlGenerator Generator { get; set; }

    [HtmlAttributeName(FOR_ATTRIBUTE_NAME)]
    public ModelExpression For { get; set; }

    [HtmlAttributeName(DISPLAY_HINT_ATTRIBUTE_NAME)]
    public bool DisplayHint { get; set; } = true;

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; }

    #endregion

    #region Ctor

    public LabelTagHelper(IHtmlGenerator generator, ILocalizationService localizationService, IWorkContext workContext)
    {
        Generator = generator;
        _localizationService = localizationService;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        ArgumentNullException.ThrowIfNull(context);

        ArgumentNullException.ThrowIfNull(output);

        var tagBuilder = Generator.GenerateLabel(ViewContext, For.ModelExplorer, For.Name, null, new { @class = "form-label" });
        if (tagBuilder != null)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag;

            var classValue = output.Attributes.ContainsName("class")
                ? $"{output.Attributes["class"].Value} label-wrapper"
                : "label-wrapper";
            output.Attributes.SetAttribute("class", classValue);

            output.Content.SetHtmlContent(tagBuilder);

            if (DisplayHint && For.Metadata.AdditionalValues.TryGetValue("LocalizedDisplayNameAttribute", out var value)
                && value is LocalizedDisplayNameAttribute resourceDisplayName)
            {
                var language = await _workContext.GetCurrentUserLanguageAsync();
                var hintResource = await _localizationService.GetResourceAsync(language.Id, $"{resourceDisplayName.ResourceKey}.Hint", returnEmptyIfNotFound: true);

                if (!string.IsNullOrEmpty(hintResource))
                {
                    var hintContent = $"<span title='{WebUtility.HtmlEncode(hintResource)}' data-bs-toggle='tooltip' class='icon-hint'><i class='fas fa-question-circle'></i></span>";
                    output.Content.AppendHtml(hintContent);
                }
            }
        }
    }

    #endregion
}