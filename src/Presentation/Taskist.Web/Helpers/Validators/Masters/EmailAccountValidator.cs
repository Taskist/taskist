using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Masters;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.Masters;

public class EmailAccountValidator : AbstractValidator<EmailAccountModel>
{
    public EmailAccountValidator(ILocalizationService localizationService,
        IEmailAccountService emailAccountService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage(localizationService.GetResource("EmailAccountModel.Name.RequiredMsg"))
            .MaximumLength(100).WithMessage(localizationService.GetResource("EmailAccountModel.Name.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await emailAccountService.GetByNameAsync(x.Name);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await emailAccountService.GetByNameAsync(x.Name);
                return entity == null;
            }).WithMessage(localizationService.GetResource("EmailAccountModel.Name.UniqueMsg"));

        RuleFor(r => r.Description)
            .MaximumLength(250).WithMessage(localizationService.GetResource("EmailAccountModel.Description.MaxLengthMsg"));

        RuleFor(r => r.UserName)
            .NotEmpty().WithMessage(localizationService.GetResource("EmailAccountModel.UserName.RequiredMsg"))
            .MaximumLength(250).WithMessage(localizationService.GetResource("EmailAccountModel.UserName.MaxLengthMsg"));

        RuleFor(r => r.Host)
            .NotEmpty().WithMessage(localizationService.GetResource("EmailAccountModel.Host.RequiredMsg"))
            .MaximumLength(250).WithMessage(localizationService.GetResource("EmailAccountModel.Host.MaxLengthMsg"));

        RuleFor(r => r.Port)
            .NotEmpty().WithMessage(localizationService.GetResource("EmailAccountModel.Port.RequiredMsg"));

        RuleFor(r => r.Password)
            .NotEmpty().When(r => r.Id == 0).WithMessage(localizationService.GetResource("EmailAccountModel.Password.RequiredMsg"))
            .MaximumLength(250).WithMessage(localizationService.GetResource("EmailAccountModel.Password.MaxLengthMsg"));

        RuleFor(r => r.FromEmail)
            .NotEmpty().WithMessage(localizationService.GetResource("EmailAccountModel.FromEmail.RequiredMsg"))
            .EmailAddress().WithMessage(localizationService.GetResource("EmailAccountModel.FromEmail.InvalidEmailMsg"))
            .MaximumLength(250).WithMessage(localizationService.GetResource("EmailAccountModel.FromEmail.MaxLengthMsg"));

        RuleFor(r => r.FromName)
            .NotEmpty().WithMessage(localizationService.GetResource("EmailAccountModel.FromName.RequiredMsg"))
            .MaximumLength(250).WithMessage(localizationService.GetResource("EmailAccountModel.FromName.MaxLengthMsg"));
    }
}