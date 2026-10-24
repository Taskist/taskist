using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Masters;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.Masters;

public class EmailTemplateValidator : AbstractValidator<EmailTemplateModel>
{
    public EmailTemplateValidator(ILocalizationService localizationService,
        IEmailTemplateService emailTemplateService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage(localizationService.GetResource("EmailTemplateModel.Name.RequiredMsg"))
            .MaximumLength(100).WithMessage(localizationService.GetResource("EmailTemplateModel.Name.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await emailTemplateService.GetByNameAsync(x.Name);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await emailTemplateService.GetByNameAsync(x.Name);
                return entity == null;
            }).WithMessage(localizationService.GetResource("EmailTemplateModel.Name.UniqueMsg"));

        RuleFor(r => r.EmailSubject)
            .NotEmpty().WithMessage(localizationService.GetResource("EmailTemplateModel.EmailSubject.RequiredMsg"))
            .MaximumLength(250).WithMessage(localizationService.GetResource("EmailTemplateModel.EmailSubject.MaxLengthMsg"));

        RuleFor(r => r.EmailAccountId)
            .NotEmpty().WithMessage(localizationService.GetResource("EmailTemplateModel.EmailAccount.RequiredMsg"));
    }
}