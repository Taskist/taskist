using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;
using Taskist.Core.Common;
using Taskist.Core.Domain.Wiki;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Taskist.Service.Users;
using Taskist.Service.Wiki;
using Taskist.Service.WorkItems;
using Taskist.Web.Controllers.Common;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Wiki;
using Taskist.Web.Helpers.Common;
using Taskist.Web.Helpers.Wiki;

namespace Taskist.Web.Controllers.Resources;

public class WikiDocumentController : BaseController
{
    #region Fields

    protected readonly IWikiService _wikiService;
    protected readonly IWikiCategoryService _wikiCategoryService;
    protected readonly IUserService _userService;
    protected readonly IModuleService _moduleService;
    protected readonly IDocumentService _documentService;
    protected readonly IBacklogService _backlogItemService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IWorkContext _workContext;
    protected readonly IWikiAccessService _wikiAccessService;

    #endregion

    #region Ctor

    public WikiDocumentController(IWikiService wikiService,
        IWikiCategoryService wikiCategoryService,
        IUserService userService,
        IModuleService moduleService,
        IDocumentService documentService,
        IBacklogService backlogItemService,
        ILocalizationService localizationService,
        IWorkContext workContext,
        IWikiAccessService wikiAccessService)
    {
        _wikiService = wikiService;
        _wikiCategoryService = wikiCategoryService;
        _userService = userService;
        _moduleService = moduleService;
        _documentService = documentService;
        _backlogItemService = backlogItemService;
        _localizationService = localizationService;
        _workContext = workContext;
        _wikiAccessService = wikiAccessService;
    }

    #endregion

    #region Helpers

    private Task<bool> CanEditAsync(int projectId) => _wikiAccessService.CanEditProjectAsync(projectId);

    private Task<bool> CanEditPageAsync(int wikiId) => _wikiAccessService.CanEditPageAsync(wikiId);

    private static IActionResult Denied() => new JsonResult(new { success = false, message = "You don't have permission to edit this wiki." });

    #endregion

    #region Actions

    [CheckPermission(nameof(PermissionProvider.ManageWiki))]
    public async Task<IActionResult> Index(int projectId = 0)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var projects = await _userService.GetAllAccessibleProjects(loggedUser.Id);

        if (projectId <= 0 || projects.All(p => p.Id != projectId))
            projectId = projects.FirstOrDefault()?.Id ?? 0;

        var projectMap = projectId > 0 ? await _userService.GetProjectMapping(loggedUser.Id, projectId) : null;

        var model = new WikiDocumentModel
        {
            SelectedProjectId = projectId,
            HasProject = projectId > 0,
            CanAddWiki = projectMap != null && projectMap.CanAddWiki,
            AvailableProjects = projects.Select(p => new SelectListItem
            {
                Text = p.Name,
                Value = p.Id.ToString(),
                Selected = p.Id == projectId
            }).ToList()
        };

        return View(model);
    }

    #endregion

    #region Ajax

    [HttpGet]
    public async Task<IActionResult> GetProjectContext(int projectId)
    {
        var loggedUser = await _workContext.GetCurrentUserAsync();
        var projectMap = projectId > 0 ? await _userService.GetProjectMapping(loggedUser.Id, projectId) : null;

        return Json(new
        {
            hasProject = projectId > 0,
            canAddWiki = projectMap != null && projectMap.CanAddWiki
        });
    }

    [HttpPost]
    public async Task<IActionResult> AddWiki(string title, int categoryId)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Json(new { success = false, message = "Title is required." });

        var category = await _wikiCategoryService.GetByIdAsync(categoryId);
        if (category == null || !await CanEditAsync(category.ProjectId))
            return Denied();

        var entity = new WikiDocument
        {
            Title = title.Trim(),
            CategoryId = categoryId,
            ContentHtml = "",
            CreatedById = UserId,
            CreatedOn = DateTime.Now,
            ModifiedById = UserId,
            ModifiedOn = DateTime.Now,
            Deleted = false
        };

        await _wikiService.InsertAsync(entity);

        return Json(new { success = true, id = "page_" + entity.Id, text = entity.Title, type = "file", children = false });
    }

    [HttpPost]
    public async Task<IActionResult> SaveWiki(string id, string content)
    {
        if (!WikiNodeId.TryParsePage(id, out var wikiId))
            return Json(new { success = false, message = "Wiki page not found." });

        if (!await CanEditPageAsync(wikiId))
            return Denied();

        var wiki = await _wikiService.GetByIdAsync(wikiId);
        if (wiki == null)
            return Json(new { success = false, message = "Wiki page not found." });

        if (wiki.ContentHtml == content)
            return Json(new { success = false, message = "No changes to save." });

        wiki.ContentHtml = content;
        wiki.ModifiedOn = DateTime.Now;
        wiki.ModifiedById = UserId;

        await _wikiService.UpdateAsync(wiki, "Edited content");

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> RenameWiki(string id, string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return Json(new { success = false, message = "Title is required." });

        if (!WikiNodeId.TryParsePage(id, out var wikiId))
            return Json(new { success = false, message = "Wiki page not found." });

        if (!await CanEditPageAsync(wikiId))
            return Denied();

        var wiki = await _wikiService.GetByIdAsync(wikiId);
        if (wiki == null)
            return Json(new { success = false, message = "Wiki page not found." });

        wiki.Title = title.Trim();
        wiki.ModifiedOn = DateTime.Now;
        wiki.ModifiedById = UserId;
        await _wikiService.UpdateAsync(wiki);

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> MoveWiki(string id, string categoryId)
    {
        if (!WikiNodeId.TryParsePage(id, out var wikiId))
            return Json(new { success = false, message = "Wiki page not found." });
        if (!WikiNodeId.TryParseCategory(categoryId, out var targetCategoryId))
            return Json(new { success = false, message = "Wiki category not found." });

        if (!await CanEditPageAsync(wikiId))
            return Denied();

        var wiki = await _wikiService.GetByIdAsync(wikiId);
        if (wiki == null)
            return Json(new { success = false, message = "Wiki page not found." });

        var currentCategory = await _wikiCategoryService.GetByIdAsync(wiki.CategoryId);
        if (currentCategory == null || !await _wikiAccessService.IsCategoryInProjectAsync(targetCategoryId, currentCategory.ProjectId))
            return Json(new { success = false, message = "Wiki category not found." });

        wiki.CategoryId = targetCategoryId;
        wiki.ModifiedOn = DateTime.Now;
        wiki.ModifiedById = UserId;
        await _wikiService.UpdateAsync(wiki);

        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteWiki(string id)
    {
        if (!WikiNodeId.TryParsePage(id, out var wikiId))
            return Json(new { success = false, message = "Wiki page not found." });

        if (!await CanEditPageAsync(wikiId))
            return Denied();

        var deleted = await _wikiService.DeleteAsync(wikiId);

        return Json(new { success = deleted });
    }

    [HttpGet]
    public async Task<IActionResult> GetContent(string id)
    {
        if (!WikiNodeId.TryParsePage(id, out var wikiId) || !await _wikiAccessService.CanViewPageAsync(wikiId))
            return Json(new { content = "" });

        var wikiPage = await _wikiService.GetByIdAsync(wikiId);
        if (wikiPage == null)
            return Json(new { content = "" });

        return Json(new
        {
            content = wikiPage.ContentHtml,
            modifiedBy = wikiPage.ModifiedBy?.Name,
            modifiedOn = wikiPage.ModifiedOn.ToString("dd-MMM-yyyy hh:mm tt")
        });
    }

    [HttpGet]
    [CheckPermission(nameof(PermissionProvider.ManageWiki))]
    public async Task<IActionResult> ExportWord(int id)
    {
        if (!await _wikiAccessService.CanViewPageAsync(id))
            return NotFound();

        var wikiPage = await _wikiService.GetByIdAsync(id);
        if (wikiPage == null)
            return NotFound();

        var html = await EmbedImagesAsync(wikiPage.ContentHtml ?? string.Empty);

        using var stream = new MemoryStream();
        using (var word = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = word.AddMainDocumentPart();
            mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document(
                new DocumentFormat.OpenXml.Wordprocessing.Body());

            ApplyBaseStyles(mainPart);

            var body = mainPart.Document.Body!;
            body.AppendChild(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                new DocumentFormat.OpenXml.Wordprocessing.Run(
                    new DocumentFormat.OpenXml.Wordprocessing.RunProperties(
                        new DocumentFormat.OpenXml.Wordprocessing.RunFonts { Ascii = WordFont, HighAnsi = WordFont, ComplexScript = WordFont },
                        new DocumentFormat.OpenXml.Wordprocessing.Bold(),
                        new DocumentFormat.OpenXml.Wordprocessing.FontSize { Val = "44" }),
                    new DocumentFormat.OpenXml.Wordprocessing.Text(wikiPage.Title ?? string.Empty))));

            var converter = new HtmlToOpenXml.HtmlConverter(mainPart);
            await converter.ParseBody(html);
            mainPart.Document.Save();
        }

        stream.Position = 0;
        return File(stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            $"{SafeFileName(wikiPage.Title)}.docx");
    }

    #region Export helpers

    private const string WordFont = "Calibri";
    private const string WordFontSize = "20";

    private static void ApplyBaseStyles(MainDocumentPart mainPart)
    {
        var stylePart = mainPart.AddNewPart<StyleDefinitionsPart>();
        var runFonts = new DocumentFormat.OpenXml.Wordprocessing.RunFonts { Ascii = WordFont, HighAnsi = WordFont, ComplexScript = WordFont };

        var docDefaults = new DocumentFormat.OpenXml.Wordprocessing.DocDefaults(
            new DocumentFormat.OpenXml.Wordprocessing.RunPropertiesDefault(
                new DocumentFormat.OpenXml.Wordprocessing.RunPropertiesBaseStyle(
                    (DocumentFormat.OpenXml.Wordprocessing.RunFonts)runFonts.CloneNode(true),
                    new DocumentFormat.OpenXml.Wordprocessing.FontSize { Val = WordFontSize },
                    new DocumentFormat.OpenXml.Wordprocessing.FontSizeComplexScript { Val = WordFontSize })));

        var normal = new DocumentFormat.OpenXml.Wordprocessing.Style(
            new DocumentFormat.OpenXml.Wordprocessing.StyleName { Val = "Normal" },
            new DocumentFormat.OpenXml.Wordprocessing.StyleRunProperties(
                (DocumentFormat.OpenXml.Wordprocessing.RunFonts)runFonts.CloneNode(true),
                new DocumentFormat.OpenXml.Wordprocessing.FontSize { Val = WordFontSize },
                new DocumentFormat.OpenXml.Wordprocessing.FontSizeComplexScript { Val = WordFontSize }))
        {
            Type = DocumentFormat.OpenXml.Wordprocessing.StyleValues.Paragraph,
            StyleId = "Normal",
            Default = true
        };

        stylePart.Styles = new DocumentFormat.OpenXml.Wordprocessing.Styles(docDefaults, normal);
        stylePart.Styles.Save();
    }

    private async Task<string> EmbedImagesAsync(string html)
    {
        if (string.IsNullOrEmpty(html))
            return html;

        var matches = Regex.Matches(html, "<img[^>]*?src=[\"']([^\"']+)[\"']", RegexOptions.IgnoreCase);
        foreach (Match match in matches)
        {
            var src = match.Groups[1].Value;
            var docId = WikiImageReference.ExtractId(src);
            if (docId == null || !await _wikiAccessService.CanViewImageAsync(docId.Value))
                continue;

            var document = await _documentService.GetByIdAsync(docId.Value);
            var imageType = UploadPolicy.ImageContentType(document?.Extension);
            if (document?.FileData == null || document.FileData.Length == 0 || imageType == null)
                continue;

            var dataUri = $"data:{imageType};base64,{Convert.ToBase64String(document.FileData)}";
            html = html.Replace(src, dataUri);
        }

        return html;
    }

    private static string SafeFileName(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "wiki";

        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(title.Where(c => !invalid.Contains(c)).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? "wiki" : cleaned;
    }

    #endregion

    [HttpGet]
    public async Task<IActionResult> GetModules(int projectId)
    {
        var modules = await _moduleService.GetAllActiveByProjectAsync(projectId);

        return Json(modules.OrderBy(x => x.Name)
            .Select(x => new { id = x.Id, text = x.Name }));
    }

    [HttpGet]
    public async Task<IActionResult> GetTags(string id)
    {
        if (!WikiNodeId.TryParsePage(id, out var wikiId) || !await _wikiAccessService.CanViewPageAsync(wikiId))
            return Json(new { });

        var wiki = await _wikiService.GetByIdAsync(wikiId);
        if (wiki == null)
            return Json(new { });

        return Json(new
        {
            moduleId = wiki.ModuleId,
            subModuleId = wiki.SubModuleId,
            moduleName = wiki.Module?.Name,
            subModuleName = wiki.SubModule?.Name
        });
    }

    [HttpPost]
    public async Task<IActionResult> SaveTags(string id, int? moduleId, int? subModuleId)
    {
        if (!WikiNodeId.TryParsePage(id, out var wikiId))
            return Json(new { success = false, message = "Wiki page not found." });

        if (!await CanEditPageAsync(wikiId))
            return Denied();

        var wiki = await _wikiService.GetByIdAsync(wikiId);
        if (wiki == null)
            return Json(new { success = false, message = "Wiki page not found." });

        wiki.ModuleId = moduleId == 0 ? null : moduleId;
        wiki.SubModuleId = subModuleId == 0 ? null : subModuleId;
        wiki.ModifiedOn = DateTime.Now;
        wiki.ModifiedById = UserId;
        await _wikiService.UpdateAsync(wiki);

        return Json(new { success = true });
    }

    [HttpPost]
    [CheckPermission(nameof(PermissionProvider.ManageWiki))]
    public async Task<IActionResult> UploadImage(IFormFile file, string pageId)
    {
        if (file == null || file.Length == 0)
            return Json(new { error = "No file uploaded." });

        var projectId = WikiNodeId.TryParsePage(pageId, out var wikiId) ? await _wikiAccessService.GetEditablePageProjectAsync(wikiId) : null;
        if (projectId == null)
            return Json(new { error = "Open a wiki page you can edit before adding images." });

        if (!UploadPolicy.IsAllowedImage(file.FileName, file.Length, UploadPolicy.MaxImageMegabytes))
            return Json(new { error = $"Only PNG, JPEG, GIF, WebP or BMP images up to {UploadPolicy.MaxImageMegabytes} MB can be uploaded." });

        var document = await _documentService.InsertAsync(file);
        await _wikiService.InsertImageAsync(new WikiImage
        {
            DocumentId = document.Id,
            ProjectId = projectId.Value,
            WikiDocumentId = wikiId,
            UploadedById = UserId
        });

        return Json(new { location = Url.Action("Image", "WikiDocument", new { id = document.Id }) });
    }

    [HttpGet]
    [CheckPermission(nameof(PermissionProvider.ManageWiki))]
    public async Task<IActionResult> Image(int id)
    {
        var document = await _wikiAccessService.CanViewImageAsync(id) ? await _documentService.GetByIdAsync(id) : null;
        if (document == null)
            return NotFound();

        return ImageFile(document);
    }

    [HttpGet]
    public async Task<IActionResult> GetVersions(string id)
    {
        if (!WikiNodeId.TryParsePage(id, out var wikiId) || !await _wikiAccessService.CanViewPageAsync(wikiId))
            return Json(Array.Empty<object>());

        var versions = await _wikiService.GetVersionsAsync(wikiId);

        return Json(versions.Select(v => new
        {
            id = v.Id,
            modifiedOn = v.CreatedOn.ToString("dd-MMM-yyyy hh:mm tt"),
            modifiedBy = v.ModifiedBy?.Name,
            note = v.Note
        }));
    }

    [HttpPost]
    public async Task<IActionResult> RestoreVersion(string id, int versionId)
    {
        if (!WikiNodeId.TryParsePage(id, out var wikiId))
            return Json(new { success = false, message = "Wiki page not found." });

        if (!await CanEditPageAsync(wikiId))
            return Denied();

        if (!await _wikiService.RestoreVersionAsync(wikiId, versionId, UserId))
            return Json(new { success = false, message = "Version not found." });

        return Json(new { success = true });
    }

    [HttpGet]
    public async Task<IActionResult> GetLinkedTasks(string id)
    {
        if (!WikiNodeId.TryParsePage(id, out var wikiId) || !await _wikiAccessService.CanViewPageAsync(wikiId))
            return Json(Array.Empty<object>());

        var tasks = await _backlogItemService.GetBySourceWikiDocumentAsync(wikiId);

        return Json(tasks.Select(t => new
        {
            id = t.Id,
            title = t.Title,
            status = t.Status?.Name
        }));
    }

    #endregion
}
