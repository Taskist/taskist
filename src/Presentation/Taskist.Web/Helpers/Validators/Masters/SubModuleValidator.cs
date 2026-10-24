using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Masters;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.Masters;

public class SubModuleValidator : AbstractValidator<SubModuleModel>
{
	public SubModuleValidator(ILocalizationService localizationService,
		ISubModuleService subModuleService)
	{
		RuleFor(r => r.Name)
			.NotEmpty().WithMessage(localizationService.GetResource("SubModuleModel.Name.RequiredMsg"))
			.MaximumLength(100).WithMessage(localizationService.GetResource("SubModuleModel.Name.MaxLengthMsg"))
			.MustAwait(async (x, context) =>
			{
				if (x.Id > 0)
				{
					var editedEntity = await subModuleService.GetByNameAsync(x.Name, x.ModuleId);
					return editedEntity == null || editedEntity.Id == x.Id;
				}
				var entity = await subModuleService.GetByNameAsync(x.Name, x.ModuleId);
				return entity == null;
			}).WithMessage(localizationService.GetResource("SubModuleModel.Name.UniqueMsg"));

		RuleFor(r => r.Description)
			.MaximumLength(250).WithMessage(localizationService.GetResource("SubModuleModel.Description.MaxLengthMsg"));
	}
}