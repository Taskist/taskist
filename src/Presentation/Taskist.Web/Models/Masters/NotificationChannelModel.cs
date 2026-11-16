using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters;

public class NotificationChannelModel : BaseModel
{
    [LocalizedDisplayName("NotificationChannelModel.Name")]
    public string? Name { get; set; }

    [LocalizedDisplayName("NotificationChannelModel.ChannelType")]
    public int ChannelType { get; set; } = 1;

    [LocalizedDisplayName("NotificationChannelModel.EndpointUrl")]
    public string? EndpointUrl { get; set; }

    public bool HasEndpointUrl { get; set; }

    [LocalizedDisplayName("NotificationChannelModel.SigningSecret")]
    public string? SigningSecret { get; set; }

    public bool HasSigningSecret { get; set; }

    [LocalizedDisplayName("NotificationChannelModel.Active")]
    public bool Active { get; set; } = true;

    public DateTime? LastSentOn { get; set; }

    public string? LastError { get; set; }

    public IList<SelectListItem> AvailableTypes { get; set; } = [];
}
