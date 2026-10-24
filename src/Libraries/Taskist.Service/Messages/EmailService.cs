using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Settings;
using System.Net;
using System.Net.Mail;

namespace Taskist.Service.Messages;

public class EmailService : IEmailService
{
	private readonly IAppLinkBuilder _linkBuilder;
	private readonly GeneralSettings _generalSettings;

	public EmailService(IAppLinkBuilder linkBuilder, GeneralSettings generalSettings)
	{
		_linkBuilder = linkBuilder;
		_generalSettings = generalSettings;
	}

	public async Task SendEmailAsync(EmailAccount emailAccount,
		string subject,
		string body,
		string toAddress,
		string toName)
	{
		try
		{
			body = EmailLayoutBuilder.Wrap(body, _linkBuilder.Build("images/brand/logo.png"), _generalSettings.CompanyName);

			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
					| SecurityProtocolType.Tls11
					| SecurityProtocolType.Tls12;

			using MailMessage message = new()
			{
				From = new MailAddress(emailAccount.FromEmail, emailAccount.FromName)
			};
			message.To.Add(new MailAddress(toAddress, toName));
			message.Subject = subject;
			message.IsBodyHtml = true;
			message.Body = body;

			using SmtpClient smtp = new();

			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
				   | SecurityProtocolType.Tls11
				   | SecurityProtocolType.Tls12;
			smtp.Port = emailAccount.Port;
			smtp.Host = emailAccount.Host;
			smtp.EnableSsl = emailAccount.EnableSsl;
			smtp.UseDefaultCredentials = false;
			smtp.Credentials = new NetworkCredential(emailAccount.UserName, emailAccount.Password);
			smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
			smtp.Send(message);
		}
		catch (Exception ex)
		{
			throw new Exception(ex.Message, ex);
		}
	}
}
