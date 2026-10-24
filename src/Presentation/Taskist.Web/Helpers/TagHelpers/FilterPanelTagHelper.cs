using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Encodings.Web;
using Taskist.Core.Common;
using Taskist.Service.Localization;

namespace Taskist.Web.Helpers.TagHelpers;

[HtmlTargetElement("wp-filter-panel", Attributes = "id", TagStructure = TagStructure.NormalOrSelfClosing)]
public class FilterPanelTagHelper : TagHelper
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public FilterPanelTagHelper(ILocalizationService localizationService, IWorkContext workContext)
    {
        _localizationService = localizationService;
        _workContext = workContext;
    }

    #endregion

    #region Properties

    public string Id { get; set; }

    public string Title { get; set; }

    #endregion

    #region Methods

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var body = (await output.GetChildContentAsync()).GetContent();

        var label = Title;
        if (string.IsNullOrEmpty(label))
        {
            var language = await _workContext.GetCurrentUserLanguageAsync();
            label = await _localizationService.GetResourceAsync(language.Id, "Filter.Toggle", returnEmptyIfNotFound: true);
            if (string.IsNullOrEmpty(label))
                label = "Filters";
        }

        var e = HtmlEncoder.Default;
        var id = e.Encode(Id);
        var collapseId = id + "Collapse";
        var buttonId = id + "Btn";
        var labelEnc = e.Encode(label);

        output.TagName = "div";
        output.TagMode = TagMode.StartTagAndEndTag;
        output.Attributes.SetAttribute("class", "wp-filter-wrap mb-3");

        output.Content.SetHtmlContent(
            $"<div class=\"d-flex justify-content-end mb-2\">" +
            $"<button type=\"button\" class=\"btn btn-sm btn-outline-secondary wp-filter-btn\" id=\"{buttonId}\" " +
            $"data-bs-toggle=\"collapse\" data-bs-target=\"#{collapseId}\" aria-expanded=\"false\">" +
            $"<i class=\"fas fa-sliders me-1\"></i>{labelEnc}<span class=\"wp-filter-count\" id=\"{id}Count\"></span></button></div>" +
            $"<div class=\"collapse\" id=\"{collapseId}\">" +
            $"<div class=\"card wp-filter\"><div class=\"card-body p-3\" id=\"{id}\">" +
            body +
            $"</div></div></div>");
    }

    #endregion
}
