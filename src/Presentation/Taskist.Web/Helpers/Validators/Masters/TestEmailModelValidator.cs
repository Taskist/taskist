using Taskist.Service.Localization;
using Taskist.Web.Models.Masters;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.Masters;

public class TestEmailModelValidator : AbstractValidator<TestEmailModel>
{
    public TestEmailModelValidator(ILocalizationService localizationService)
    {
        RuleFor(r => r.ToEmail)
            .NotEmpty().WithMessage(localizationService.GetResource("TestEmailModel.ToEmail.RequiredMsg"))
            .EmailAddress().WithMessage(localizationService.GetResource("TestEmailModel.ToEmail.InvalidEmailMsg"));
    }
}
