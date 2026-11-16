using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Models.Logging;

public class LogViewModel
{
    public DateOnly? From { get; set; }

    public DateOnly? To { get; set; }

    public int? UserId { get; set; }

    public string Type { get; set; }

    public string Action { get; set; }

    public List<SelectListItem> Users { get; set; } = [];

    public List<SelectListItem> Types { get; set; } = [];

    public List<SelectListItem> Actions { get; set; } = [];
}
