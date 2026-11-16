using Taskist.Core.Enums;
using Taskist.Service.Localization;
using Taskist.Web.Models.Masters;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.Masters;

public class NotificationChannelValidator : AbstractValidator<NotificationChannelModel>
{
    public NotificationChannelValidator(ILocalizationService localizationService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage(localizationService.GetResource("NotificationChannelModel.Name.RequiredMsg"))
            .MaximumLength(100);

        RuleFor(r => r.ChannelType)
            .Must(v => Enum.IsDefined(typeof(NotificationChannelType), v));

        RuleFor(r => r.EndpointUrl)
            .NotEmpty().When(r => r.Id == 0).WithMessage(localizationService.GetResource("NotificationChannelModel.EndpointUrl.RequiredMsg"))
            .MaximumLength(2000);

        RuleFor(r => r.SigningSecret)
            .MaximumLength(500);
    }
}
