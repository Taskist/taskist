using Hangfire;
using System.Net;
using System.Text.RegularExpressions;
using Taskist.Core.Common;
using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Extensions;
using Taskist.Service.Masters;
using Taskist.Service.Security;

namespace Taskist.Service.Messages;

public class MessageService : IMessageService
{
    #region Fields

    private readonly IEmailAccountService _emailAccounteService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly IEmailService _emailSenderService;
    private readonly IEncryptionService _encryptionService;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IHttpHelper _httpHelper;

    #endregion

    #region Ctor

    public MessageService(IEmailAccountService emailAccountService,
        IEmailTemplateService emailTemplateService,
        IEmailService emailSenderService,
        IHttpHelper httpHelper,
        IBackgroundJobClient backgroundJobClient,
        IEncryptionService encryptionService)
    {
        _emailAccounteService = emailAccountService;
        _emailTemplateService = emailTemplateService;
        _emailSenderService = emailSenderService;
        _backgroundJobClient = backgroundJobClient;
        _httpHelper = httpHelper;
        _encryptionService = encryptionService;
    }

    #endregion

    #region Account

    public async Task EmailPasswordResetLinkAsync(User user, string token)
    {
        var emailTemplate = await _emailTemplateService.GetByNameAsync(EmailTemplateTypeEnum.ResetPassword.ToString());
        if (emailTemplate != null && !string.IsNullOrEmpty(emailTemplate.EmailBody))
        {
            var emailAccount = await _emailAccounteService.GetByIdAsync(emailTemplate.EmailAccountId);
            if (emailAccount.Active)
            {
                var url = $"{_httpHelper.GetBaseURL()}/setpassword?token={WebUtility.UrlEncode(token)}";
                var body = emailTemplate.EmailBody;
                body = body.Replace("%displayname%", $"{user.FirstName} {user.LastName}");
                body = body.Replace("%url%", url);
                body = body.Replace("%year%", DateTime.Today.Year.ToString());
                body = body.Replace("%company%", Constant.CompanyName);

                await _emailSenderService.SendEmailAsync(emailAccount, emailTemplate.EmailSubject, body,
                    user.Email, $"{user.FirstName} {user.LastName}");
            }
        }
    }

    public async Task EmailActivationKitAsync(User user)
    {
        var emailTemplate = await _emailTemplateService.GetByNameAsync(EmailTemplateTypeEnum.Activation.ToString());
        if (emailTemplate != null && !string.IsNullOrEmpty(emailTemplate.EmailBody))
        {
            var emailAccount = await _emailAccounteService.GetByIdAsync(emailTemplate.EmailAccountId);
            if (emailAccount.Active)
            {
                var token = _encryptionService.GenerateToken(user.Code);
                var url = $"{_httpHelper.GetBaseURL()}/activate?token={token}";
                var body = emailTemplate.EmailBody;
                body = body.Replace("%display_name%", $"{user.FirstName} {user.LastName}");
                body = body.Replace("%body%", $"Please click the link below to activate your account.");
                body = body.Replace("%url%", url);
                body = body.Replace("%year%", DateTime.Today.Year.ToString());
                body = body.Replace("%company%", Constant.CompanyName);

                await _emailSenderService.SendEmailAsync(emailAccount, emailTemplate.EmailSubject, body,
                    user.Email, $"{user.FirstName} {user.LastName}");
            }
        }
    }

    public async Task EmailWelcomeKitAsync(User user)
    {
        var emailTemplate = await _emailTemplateService.GetByNameAsync(EmailTemplateTypeEnum.WelcomeKit.ToString());
        if (emailTemplate != null && !string.IsNullOrEmpty(emailTemplate.EmailBody))
        {
            var emailAccount = await _emailAccounteService.GetByIdAsync(emailTemplate.EmailAccountId);
            if (emailAccount.Active)
            {
                var url = $"{_httpHelper.GetBaseURL()}";
                var body = emailTemplate.EmailBody;
                body = body.Replace("%display_name%", $"{user.FirstName} {user.LastName}");
                body = body.Replace("%button%", url);
                body = body.Replace("%year%", DateTime.Today.Year.ToString());
                body = body.Replace("%company%", Constant.CompanyName);

                await _emailSenderService.SendEmailAsync(emailAccount, emailTemplate.EmailSubject, body,
                     user.Email, $"{user.FirstName} {user.LastName}");
            }
        }
    }

    public async Task EmailRegistrationKitAsync(User user)
    {
        var emailTemplate = await _emailTemplateService.GetByNameAsync(EmailTemplateTypeEnum.RegistrationKit.ToString());
        if (emailTemplate != null && !string.IsNullOrEmpty(emailTemplate.EmailBody))
        {
            var emailAccount = await _emailAccounteService.GetByIdAsync(emailTemplate.EmailAccountId);
            if (emailAccount.Active)
            {
                var url = $"{_httpHelper.GetBaseURL()}";
                var body = emailTemplate.EmailBody;
                body = body.Replace("%display_name%", $"{user.FirstName} {user.LastName}");
                body = body.Replace("%button%", url);
                body = body.Replace("%year%", DateTime.Today.Year.ToString());
                body = body.Replace("%company%", Constant.CompanyName);

                await _emailSenderService.SendEmailAsync(emailAccount, emailTemplate.EmailSubject, body,
                    user.Email, $"{user.FirstName} {user.LastName}");
            }
        }
    }

    public async Task EmailNotificationAsync(User user, string content)
    {
        var emailTemplate = await _emailTemplateService.GetByNameAsync(EmailTemplateTypeEnum.Notification.ToString());
        if (emailTemplate != null && !string.IsNullOrEmpty(emailTemplate.EmailBody))
        {
            var emailAccount = await _emailAccounteService.GetByIdAsync(emailTemplate.EmailAccountId);
            if (emailAccount.Active)
            {
                var url = $"{_httpHelper.GetBaseURL()}";
                var body = emailTemplate.EmailBody;
                body = body.Replace("%display_name%", $"{user.FirstName} {user.LastName}");
                body = body.Replace("%button%", url);
                body = body.Replace("%year%", DateTime.Today.Year.ToString());
                body = body.Replace("%company%", Constant.CompanyName);
                body = body.Replace("%body%", content);

                await _emailSenderService.SendEmailAsync(emailAccount, emailTemplate.EmailSubject, body,
                    user.Email, $"{user.FirstName} {user.LastName}");
            }
        }
    }

    #endregion

    #region Backlog

    public async Task<Task> EmailNewTaskNotificationAsync(Backlog backlog)
    {
        var emailTemplate = await _emailTemplateService.GetByNameAsync(EmailTemplateTypeEnum.NewTaskNotification.ToString());
        if (emailTemplate != null && !string.IsNullOrEmpty(emailTemplate.EmailBody))
        {
            var emailAccount = await _emailAccounteService.GetByIdAsync(emailTemplate.EmailAccountId);
            if (emailAccount.Active)
            {
                var url = $"{_httpHelper.GetBaseURL()}/backlog/edit/{backlog.Id}";

                var tokenValues = new Dictionary<string, string>
                {
                    { "url", url },
                    { "display_name", backlog.Assignee.Name },
                    { "task_type", backlog.TaskType.Name },
                    { "title", backlog.Title },
                    { "project_name", backlog.Project.Name },
                    { "module_name", backlog.Module.Name },
                    { "sub_module_name", backlog.SubModule.Name },
                    { "severity_name", backlog.Severity.Name },
                    { "due_date", backlog.DueDate != null ? backlog.DueDate.ToDateString() : "N/A" },
                };

                var body = ReplaceTokensRegex(emailTemplate.EmailBody, tokenValues);
                var subject = $"New {backlog.TaskType.Name} assigned";

                _backgroundJobClient.Enqueue(() => _emailSenderService.SendEmailAsync(emailAccount, subject, body,
                    backlog.Assignee.Email, backlog.Assignee.Name));
            }
        }

        return Task.CompletedTask;
    }

    public async Task<Task> EmailTaskNotificationAsync(Backlog backlog, User loggedUser, string status)
    {
        var emailTemplate = await _emailTemplateService.GetByNameAsync(EmailTemplateTypeEnum.TaskNotification.ToString());
        if (emailTemplate != null && !string.IsNullOrEmpty(emailTemplate.EmailBody))
        {
            var emailAccount = await _emailAccounteService.GetByIdAsync(emailTemplate.EmailAccountId);
            if (emailAccount.Active)
            {
                var url = $"{_httpHelper.GetBaseURL()}/backlog/edit/{backlog.Id}";

                var tokenValues = new Dictionary<string, string>
                {
                    { "url", url },
                    { "display_name", backlog.Assignee.Name },
                    { "title", backlog.Title },
                    { "new_status", status },
                    { "updated_by", loggedUser.Name }
                };

                if (backlog.Project != null)
                    tokenValues.Add("project_name", backlog.Project.Name);
                if (backlog.Module != null)
                    tokenValues.Add("module_name", backlog.Module.Name);
                if (backlog.SubModule != null)
                    tokenValues.Add("sub_module_name", backlog.SubModule.Name);

                var body = ReplaceTokensRegex(emailTemplate.EmailBody, tokenValues);
                var subject = $"New {backlog.TaskType.Name} assigned";

                _backgroundJobClient.Enqueue(() => _emailSenderService.SendEmailAsync(emailAccount, subject, body,
                    backlog.Assignee.Email, backlog.Assignee.Name));
            }
        }

        return Task.CompletedTask;
    }

    #endregion

    #region Helpers

    private static string ReplaceTokensRegex(string template, Dictionary<string, string> values)
    {
        return Regex.Replace(template, @"\{(\w+)\}", match =>
            values.TryGetValue(match.Groups[1].Value, out string value) ? value : match.Value);
    }

    #endregion
}