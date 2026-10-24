using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Masters;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.Masters;

public class CustomFieldValidator : AbstractValidator<CustomFieldModel>
{
	public CustomFieldValidator(ILocalizationService localizationService,
		ICustomFieldService customFieldService)
	{
		RuleFor(r => r.Label)
			.NotEmpty().WithMessage(localizationService.GetResource("CustomFieldModel.Label.RequiredMsg"))
			.MaximumLength(100).WithMessage(localizationService.GetResource("CustomFieldModel.Label.MaxLengthMsg"))
			.MustAwait(async (x, context) =>
			{
				if (x.Id > 0)
				{
					var editedEntity = await customFieldService.GetByLabelAsync(x.ProjectId, x.Label);
					return editedEntity == null || editedEntity.Id == x.Id;
				}
				var entity = await customFieldService.GetByLabelAsync(x.ProjectId, x.Label);
				return entity == null;
			}).WithMessage(localizationService.GetResource("CustomFieldModel.Label.UniqueMsg"));

		RuleFor(r => r.ResourceKey)
			.NotEmpty().WithMessage(localizationService.GetResource("CustomFieldModel.ResourceKey.RequiredMsg"));

		RuleFor(r => r.FieldType)
			.NotEmpty().WithMessage(localizationService.GetResource("CustomFieldModel.FieldType.RequiredMsg"));

		RuleFor(r => r.ColumnClass)
			.NotEmpty().WithMessage(localizationService.GetResource("CustomFieldModel.ColumnClass.RequiredMsg"));

		RuleFor(r => r.Placement)
			.NotEmpty().WithMessage(localizationService.GetResource("CustomFieldModel.Placement.RequiredMsg"));
	}
}