using Taskist.Service.Localization;
using Taskist.Web.Models.Masters;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.Masters;

public class ProjectNotificationChannelValidator : AbstractValidator<ProjectNotificationChannelModel>
{
    public ProjectNotificationChannelValidator(ILocalizationService localizationService)
    {
        RuleFor(r => r.NotificationChannelId)
            .GreaterThan(0).WithMessage(localizationService.GetResource("ProjectNotificationChannelModel.NotificationChannelId.RequiredMsg"));

        RuleFor(r => r.Events)
            .NotEmpty().WithMessage(localizationService.GetResource("ProjectNotificationChannelModel.Events.RequiredMsg"));
    }
}
