using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Taskist.Web.Models.Masters;

public class EmailTemplateModel : BaseModel
{
    public EmailTemplateModel()
    {
        AvailableEmailAccounts = [new SelectListItem { Text = "Select", Value = "" }];
    }

    [LocalizedDisplayName("EmailAccountModel.Name")]
    public string Name { get; set; }

    [LocalizedDisplayName("EmailAccountModel.EmailSubject")]
    public string EmailSubject { get; set; }

    [LocalizedDisplayName("EmailAccountModel.EmailBody")]
    public string EmailBody { get; set; }

    [LocalizedDisplayName("EmailAccountModel.EmailAccount")]
    public int EmailAccountId { get; set; }

    [LocalizedDisplayName("EmailAccountModel.Active")]
    public bool Active { get; set; } = true;

    public IList<SelectListItem> AvailableEmailAccounts { get; set; }
}