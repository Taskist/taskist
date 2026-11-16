using Taskist.Service.Localization;
using Taskist.Web.Models.WorkItems;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.WorkItems
{
	public class BacklogBatchUpdateValidator : AbstractValidator<BacklogBatchUpdateModel>
	{
		public BacklogBatchUpdateValidator(ILocalizationService localizationService)
		{
			RuleFor(r => r.SubModuleId)
			   .NotEmpty().WithMessage(localizationService.GetResource("BacklogModel.SubModule.RequiredMsg"))
			   .When(x => x.ModuleId > 0);
		}
	}
}