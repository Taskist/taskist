using FluentValidation;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Helpers.Validators.Masters;

public class LabelValidator : AbstractValidator<LabelModel>
{
    public LabelValidator(ILocalizationService localizationService,
        IStatusService severityService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("StatusModel.Name.RequiredMsg"))
            .MaximumLength(100).WithMessageAwait(localizationService.GetResourceAsync("StatusModel.Name.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await severityService.GetByNameAsync(x.Name);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await severityService.GetByNameAsync(x.Name);
                return entity == null;
            }).WithMessageAwait(localizationService.GetResourceAsync("StatusModel.Name.UniqueMsg"));

        RuleFor(r => r.Description)
            .MaximumLength(250).WithMessageAwait(localizationService.GetResourceAsync("StatusModel.Description.MaxLengthMsg"));

        RuleFor(r => r.Color)
            .MaximumLength(20).WithMessageAwait(localizationService.GetResourceAsync("StatusModel.TextColor.MaxLengthMsg"));

      
    }
}