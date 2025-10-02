using FluentValidation;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Masters;

namespace Taskist.Web.Helpers.Validators.Masters;

public class CustomFieldValidator : AbstractValidator<CustomFieldModel>
{
    public CustomFieldValidator(ILocalizationService localizationService,
        ICustomFieldService customFieldService)
    {
        RuleFor(r => r.Label)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("CustomFieldModel.Label.RequiredMsg"))
            .MaximumLength(100).WithMessageAwait(localizationService.GetResourceAsync("CustomFieldModel.Label.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await customFieldService.GetByLabelAsync(x.ProjectId, x.Label);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await customFieldService.GetByLabelAsync(x.ProjectId, x.Label);
                return entity == null;
            }).WithMessageAwait(localizationService.GetResourceAsync("CustomFieldModel.Label.UniqueMsg"));

        RuleFor(r => r.ResourceKey)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("CustomFieldModel.ResourceKey.RequiredMsg"));

        RuleFor(r => r.FieldType)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("CustomFieldModel.FieldType.RequiredMsg"));

        RuleFor(r => r.ColumnClass)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("CustomFieldModel.ColumnClass.RequiredMsg"));

        RuleFor(r => r.Placement)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("CustomFieldModel.Placement.RequiredMsg"));
    }
}