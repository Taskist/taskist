using FluentValidation;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Helpers.Validators.Masters;

public class EmailAccountValidator : AbstractValidator<EmailAccountModel>
{
    public EmailAccountValidator(ILocalizationService localizationService,
        IEmailAccountService emailAccountService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("EmailAccountModel.Name.RequiredMsg"))
            .MaximumLength(100).WithMessageAwait(localizationService.GetResourceAsync("EmailAccountModel.Name.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await emailAccountService.GetByNameAsync(x.Name);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await emailAccountService.GetByNameAsync(x.Name);
                return entity == null;
            }).WithMessageAwait(localizationService.GetResourceAsync("EmailAccountModel.Name.UniqueMsg"));

        RuleFor(r => r.Description)
            .MaximumLength(250).WithMessageAwait(localizationService.GetResourceAsync("EmailAccountModel.Description.MaxLengthMsg"));

        RuleFor(r => r.UserName)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("EmailAccountModel.UserName.RequiredMsg"))
            .MaximumLength(250).WithMessageAwait(localizationService.GetResourceAsync("EmailAccountModel.UserName.MaxLengthMsg"));

        RuleFor(r => r.Host)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("EmailAccountModel.Host.RequiredMsg"))
            .MaximumLength(250).WithMessageAwait(localizationService.GetResourceAsync("EmailAccountModel.Host.MaxLengthMsg"));

        RuleFor(r => r.Port)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("EmailAccountModel.Port.RequiredMsg"));

        RuleFor(r => r.Password)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("EmailAccountModel.Password.RequiredMsg"))
            .MaximumLength(250).WithMessageAwait(localizationService.GetResourceAsync("EmailAccountModel.Password.MaxLengthMsg"));

        RuleFor(r => r.FromEmail)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("EmailAccountModel.FromEmail.RequiredMsg"))
            .EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("EmailAccountModel.FromEmail.InvalidEmailMsg"))
            .MaximumLength(250).WithMessageAwait(localizationService.GetResourceAsync("EmailAccountModel.FromEmail.MaxLengthMsg"));

        RuleFor(r => r.FromName)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("EmailAccountModel.FromName.RequiredMsg"))
            .MaximumLength(250).WithMessageAwait(localizationService.GetResourceAsync("EmailAccountModel.FromName.MaxLengthMsg"));
    }
}