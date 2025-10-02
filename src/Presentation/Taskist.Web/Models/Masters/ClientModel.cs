using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters;

public class ClientModel : BaseModel
{
    [LocalizedDisplayName("ClientModel.Name")]
    public string Name { get; set; }

    [LocalizedDisplayName("ClientModel.Description")]
    public string? Description { get; set; }

    [LocalizedDisplayName("ClientModel.ContactPerson")]
    public string? ContactPerson { get; set; }

    [LocalizedDisplayName("ClientModel.PhoneNumber")]
    public string? PhoneNumber { get; set; }

    [LocalizedDisplayName("ClientModel.Email")]
    public string? Email { get; set; }

    [LocalizedDisplayName("ClientModel.WebSite")]
    public string? Website { get; set; }

    [LocalizedDisplayName("ClientModel.Active")]
    public bool Active { get; set; } = true;
}
