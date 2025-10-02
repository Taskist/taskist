using Taskist.Core.Domain.Masters;

namespace Taskist.Service.Messages;

public interface IEmailService
{
	Task SendEmailAsync(EmailAccount emailAccount, string subject, string body,
	   string toAddress, string toName);
}
