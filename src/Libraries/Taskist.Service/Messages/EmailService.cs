using Taskist.Core.Domain.Masters;
using System.Net;
using System.Net.Mail;

namespace Taskist.Service.Messages;

public class EmailService : IEmailService
{
	public async Task SendEmailAsync(EmailAccount emailAccount,
		string subject,
		string body,
		string toAddress,
		string toName)
	{
		try
		{
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
