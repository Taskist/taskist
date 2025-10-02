using FluentValidation;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Helpers.Validators.Masters;

public class SeverityValidator : AbstractValidator<SeverityModel>
{
    public SeverityValidator(ILocalizationService localizationService,
        ISeverityService severityService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("SeverityModel.Name.RequiredMsg"))
            .MaximumLength(100).WithMessageAwait(localizationService.GetResourceAsync("SeverityModel.Name.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await severityService.GetByNameAsync(x.Name);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await severityService.GetByNameAsync(x.Name);
                return entity == null;
            }).WithMessageAwait(localizationService.GetResourceAsync("SeverityModel.Name.UniqueMsg"));

        RuleFor(r => r.Description)
            .MaximumLength(250).WithMessageAwait(localizationService.GetResourceAsync("SeverityModel.Description.MaxLengthMsg"));

        RuleFor(r => r.GroupId)
           .GreaterThan(0).WithMessageAwait(localizationService.GetResourceAsync("SeverityModel.Group.RequiredMsg"));

        RuleFor(r => r.TextColor)
            .MaximumLength(20).WithMessageAwait(localizationService.GetResourceAsync("SeverityModel.TextColor.MaxLengthMsg"));

        RuleFor(r => r.BackgroundColor)
            .MaximumLength(20).WithMessageAwait(localizationService.GetResourceAsync("SeverityModel.BackgroundColor.MaxLengthMsg"));

        RuleFor(r => r.IconClass)
            .MaximumLength(50).WithMessageAwait(localizationService.GetResourceAsync("SeverityModel.IconClass.MaxLengthMsg"));
    }
}