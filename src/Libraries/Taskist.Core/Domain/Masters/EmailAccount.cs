using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Masters;

public class EmailAccount : BaseEntity, IAuditableEntity
{
	public string Name { get; set; }

	public string? Description { get; set; }

	public string UserName { get; set; }

	public string Host { get; set; }

	public int Port { get; set; }

	[NotAudited]
	[Encrypted]
	public string Password { get; set; }

	public bool EnableSsl { get; set; }

	public string FromEmail { get; set; }

	public string FromName { get; set; }

	public bool Active { get; set; }
}