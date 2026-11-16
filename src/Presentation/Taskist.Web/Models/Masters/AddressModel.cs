using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters;

public class AddressModel : BaseModel
{
    [LocalizedDisplayName("AddressModel.Address1")]
    public string Address1 { get; set; }

    [LocalizedDisplayName("AddressModel.Address2")]
    public string? Address2 { get; set; }

    [LocalizedDisplayName("AddressModel.City")]
    public string? City { get; set; }

    [LocalizedDisplayName("AddressModel.ZipPostalCode")]
    public string? ZipPostalCode { get; set; }
}
