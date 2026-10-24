using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace Taskist.Web.Models.Masters;

public class TestEmailModel : BaseModel
{
    [DataType(DataType.EmailAddress)]
    [LocalizedDisplayName("TestEmailModel.ToEmail")]
    public string ToEmail { get; set; }
}
