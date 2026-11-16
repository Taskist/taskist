using FluentValidation;
using Taskist.Service.Localization;
using Taskist.Web.Models.Announcements;

namespace Taskist.Web.Helpers.Validators.Announcements;

public class AnnouncementValidator : AbstractValidator<AnnouncementModel>
{
    public AnnouncementValidator(ILocalizationService localizationService)
    {
        RuleFor(r => r.Title)
            .NotEmpty().WithMessage(localizationService.GetResource("AnnouncementModel.Title.RequiredMsg"))
            .MaximumLength(200).WithMessage(localizationService.GetResource("AnnouncementModel.Title.MaxLengthMsg"));

        RuleFor(r => r.Body)
            .NotEmpty().WithMessage(localizationService.GetResource("AnnouncementModel.Body.RequiredMsg"));

        RuleFor(r => r.ValidTo)
            .GreaterThanOrEqualTo(r => r.ValidFrom)
            .WithMessage(localizationService.GetResource("AnnouncementModel.ValidTo.RangeMsg"));
    }
}
