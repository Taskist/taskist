using Taskist.Core.Extensions;
using Taskist.Core.Enums;
using Taskist.Core.Common;
using Taskist.Core.Domain.Settings;
using Taskist.Core.Domain.WorkItems;
using Taskist.Core.Dtos.Git;
using Taskist.Core.Dtos.Notification;
using Taskist.Service.Masters;
using Taskist.Service.Security;
using Hangfire;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Taskist.Core.Domain.Users;

namespace Taskist.Service.Messages;

public class MessageService : IMessageService
{
    #region Fields

    protected readonly IEmailAccountService _emailAccounteService;
    protected readonly IEmailTemplateService _emailTemplateService;
    protected readonly IEmailService _emailSenderService;
    protected readonly IEncryptionService _encryptionService;
    protected readonly IBackgroundJobClient _backgroundJobClient;
    protected readonly IHttpHelper _httpHelper;
    protected readonly GeneralSettings _generalSettings;

    #endregion

    #region Ctor

    public MessageService(IEmailAccountService emailAccountService,
            IEmailTemplateService emailTemplateService,
            IEmailService emailSenderService,
            IHttpHelper httpHelper,
            IBackgroundJobClient backgroundJobClient,
            IEncryptionService encryptionService,
            GeneralSettings generalSettings)
    {
        _emailAccounteService = emailAccountService;
        _emailTemplateService = emailTemplateService;
        _emailSenderService = emailSenderService;
        _backgroundJobClient = backgroundJobClient;
        _httpHelper = httpHelper;
        _encryptionService = encryptionService;
        _generalSettings = generalSettings;
    }

    #endregion

    #region Account

    public async Task EmailPasswordResetLinkAsync(User user, string token)
    {
        var emailTemplate = await _emailTemplateService.GetByNameAsync(EmailTemplateType.ResetPassword.ToString());
        if (emailTemplate != null && !string.IsNullOrEmpty(emailTemplate.EmailBody))
        {
            var emailAccount = await _emailAccounteService.GetByIdAsync(emailTemplate.EmailAccountId);
            if (emailAccount.Active)
            {
                var url = $"{_httpHelper.GetBaseURL()}/setpassword?token={WebUtility.UrlEncode(token)}";
                var body = emailTemplate.EmailBody;
                body = body.Replace("%displayname%", user.Name);
                body = body.Replace("%url%", url);
                body = body.Replace("%year%", DateTime.Today.Year.ToString());
                body = body.Replace("%company%", _generalSettings.CompanyNameOrDefault());

                await _emailSenderService.SendEmailAsync(emailAccount, emailTemplate.EmailSubject, body,
                                    user.Email, user.Name);
            }
        }
    }

    public async Task EmailActivationKitAsync(User user)
    {
        var emailTemplate = await _emailTemplateService.GetByNameAsync(EmailTemplateType.Activation.ToString());
        if (emailTemplate != null && !string.IsNullOrEmpty(emailTemplate.EmailBody))
        {
            var emailAccount = await _emailAccounteService.GetByIdAsync(emailTemplate.EmailAccountId);
            if (emailAccount.Active)
            {
                var token = _encryptionService.GenerateToken(user.Code);
                var url = $"{_httpHelper.GetBaseURL()}/activate?token={token}";
                var body = emailTemplate.EmailBody;
                body = body.Replace("%display_name%", user.Name);
                body = body.Replace("%body%", $"Please click the link below to activate your account.");
                body = body.Replace("%url%", url);
                body = body.Replace("%year%", DateTime.Today.Year.ToString());
                body = body.Replace("%company%", _generalSettings.CompanyNameOrDefault());

                await _emailSenderService.SendEmailAsync(emailAccount, emailTemplate.EmailSubject, body,
                                    user.Email, user.Name);
            }
        }
    }

    public async Task EmailWelcomeKitAsync(User user)
    {
        var emailTemplate = await _emailTemplateService.GetByNameAsync(EmailTemplateType.WelcomeKit.ToString());
        if (emailTemplate != null && !string.IsNullOrEmpty(emailTemplate.EmailBody))
        {
            var emailAccount = await _emailAccounteService.GetByIdAsync(emailTemplate.EmailAccountId);
            if (emailAccount.Active)
            {
                var url = $"{_httpHelper.GetBaseURL()}";
                var body = emailTemplate.EmailBody;
                body = body.Replace("%display_name%", user.Name);
                body = body.Replace("%button%", url);
                body = body.Replace("%year%", DateTime.Today.Year.ToString());
                body = body.Replace("%company%", _generalSettings.CompanyNameOrDefault());

                await _emailSenderService.SendEmailAsync(emailAccount, emailTemplate.EmailSubject, body,
                                     user.Email, user.Name);
            }
        }
    }

    public async Task EmailRegistrationKitAsync(User user)
    {
        var emailTemplate = await _emailTemplateService.GetByNameAsync(EmailTemplateType.RegistrationKit.ToString());
        if (emailTemplate != null && !string.IsNullOrEmpty(emailTemplate.EmailBody))
        {
            var emailAccount = await _emailAccounteService.GetByIdAsync(emailTemplate.EmailAccountId);
            if (emailAccount.Active)
            {
                var url = $"{_httpHelper.GetBaseURL()}";
                var body = emailTemplate.EmailBody;
                body = body.Replace("%display_name%", user.Name);
                body = body.Replace("%button%", url);
                body = body.Replace("%year%", DateTime.Today.Year.ToString());
                body = body.Replace("%company%", _generalSettings.CompanyNameOrDefault());

                await _emailSenderService.SendEmailAsync(emailAccount, emailTemplate.EmailSubject, body,
                                    user.Email, user.Name);
            }
        }
    }

    public async Task EmailNotificationAsync(User user, string content, string? subject = null)
    {
        var emailTemplate = await _emailTemplateService.GetByNameAsync(EmailTemplateType.Notification.ToString());
        if (emailTemplate != null && !string.IsNullOrEmpty(emailTemplate.EmailBody))
        {
            var emailAccount = await _emailAccounteService.GetByIdAsync(emailTemplate.EmailAccountId);
            if (emailAccount.Active)
            {
                var url = $"{_httpHelper.GetBaseURL()}";
                var body = emailTemplate.EmailBody;
                body = body.Replace("%display_name%", user.Name);
                body = body.Replace("%button%", url);
                body = body.Replace("%year%", DateTime.Today.Year.ToString());
                body = body.Replace("%company%", _generalSettings.CompanyNameOrDefault());
                body = body.Replace("%body%", content);

                var emailSubject = string.IsNullOrWhiteSpace(subject) ? emailTemplate.EmailSubject : subject;

                await _emailSenderService.SendEmailAsync(emailAccount, emailSubject, body,
                                    user.Email, user.Name);
            }
        }
    }

    #endregion

    #region Email

    public async Task<Task> EmailNewTaskNotificationAsync(Backlog backlog, bool tester)
    {
        var emailTemplate = await _emailTemplateService.GetByNameAsync(EmailTemplateType.NewTaskNotification.ToString());
        if (emailTemplate != null && !string.IsNullOrEmpty(emailTemplate.EmailBody))
        {
            var emailAccount = await _emailAccounteService.GetByIdAsync(emailTemplate.EmailAccountId);
            if (emailAccount.Active)
            {
                var url = $"{_httpHelper.GetBaseURL()}/backlog/edit/{backlog.Id}";

                var tokenValues = new Dictionary<string, string>
                {
                    { "url", url },
                    { "task_id", backlog.Id.ToString() },
                    { "display_name", tester ? backlog.Tester.Name : backlog.Assignee.Name },
                    { "task_type", backlog.TaskType.Name },
                    { "title", backlog.Title },
                    { "project_name", backlog.Project.Name },
                    { "module_name", backlog.Module.Name },
                    { "sub_module_name", backlog.SubModule.Name },
                    { "created_by", backlog.CreatedBy.Name },
                    { "severity_name", backlog.Severity.Name },
                    { "due_date", backlog.DueDate != null ? backlog.DueDate.ToDateString() : "N/A" },
                };

                var body = ReplaceTokensRegex(emailTemplate.EmailBody, tokenValues);
                var subject = $"New {backlog.TaskType.Name} assigned";

                _backgroundJobClient.Enqueue(() => _emailSenderService.SendEmailAsync(emailAccount, subject, body,
                                    tester ? backlog.Tester.Email : backlog.Assignee.Email,
                                    tester ? backlog.Tester.Name : backlog.Assignee.Name));
            }
        }

        return Task.CompletedTask;
    }

    public async Task<Task> EmailTaskNotificationAsync(Backlog backlog, User loggedUser, string status, bool tester)
    {
        var emailTemplate = await _emailTemplateService.GetByNameAsync(EmailTemplateType.TaskNotification.ToString());
        if (emailTemplate != null && !string.IsNullOrEmpty(emailTemplate.EmailBody))
        {
            var emailAccount = await _emailAccounteService.GetByIdAsync(emailTemplate.EmailAccountId);
            if (emailAccount.Active)
            {
                var url = $"{_httpHelper.GetBaseURL()}/backlog/edit/{backlog.Id}";

                var tokenValues = new Dictionary<string, string>
                {
                    { "url", url },
                    { "display_name", tester ? backlog.Tester.Name : backlog.Assignee.Name },
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
                                    tester ? backlog.Tester.Email : backlog.Assignee.Email,
                                    tester ? backlog.Tester.Name : backlog.Assignee.Name));
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