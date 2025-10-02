using System.ComponentModel.DataAnnotations;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters;

public class EmailAccountModel : BaseModel
{
	public EmailAccountModel()
	{
		EmailTemplates = [];
	}

	[LocalizedDisplayName("EmailAccountModel.Name")]
	public string Name { get; set; }

	[LocalizedDisplayName("EmailAccountModel.Description")]
	public string? Description { get; set; }

	[LocalizedDisplayName("EmailAccountModel.UserName")]
	public string UserName { get; set; }

	[DataType(DataType.Password)]
	[LocalizedDisplayName("EmailAccountModel.Password")]
	public string Password { get; set; }

	[LocalizedDisplayName("EmailAccountModel.Host")]
	public string Host { get; set; }

	[LocalizedDisplayName("EmailAccountModel.Port")]
	public int Port { get; set; }

	[LocalizedDisplayName("EmailAccountModel.EnableSsl")]
	public bool EnableSsl { get; set; }

	[DataType(DataType.EmailAddress)]
	[LocalizedDisplayName("EmailAccountModel.FromEmail")]
	public string FromEmail { get; set; }

	[LocalizedDisplayName("EmailAccountModel.FromName")]
	public string FromName { get; set; }

	[LocalizedDisplayName("EmailAccountModel.Active")]
	public bool Active { get; set; } = true;

	public IList<EmailTemplateModel> EmailTemplates { get; set; }
}