using FluentValidation;
using Taskist.Core.Common;
using Taskist.Service.Localization;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.WorkItems;

namespace Taskist.Web.Helpers.Validators.WorkItems
{
    public class BacklogTaskValidator : AbstractValidator<BacklogModel>
    {
        public BacklogTaskValidator(IWorkContext workContext,
            ILocalizationService localizationService)
        {
            RuleFor(r => r.Title)
                .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("BacklogTaskModel.Title.RequiredMsg"))
                .MaximumLength(500).WithMessageAwait(localizationService.GetResourceAsync("BacklogTaskModel.Title.MaxLengthMsg"));

            RuleFor(r => r.ModuleId)
                .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("BacklogTaskModel.Module.RequiredMsg"));

            RuleFor(r => r.SubModuleId)
               .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("BacklogTaskModel.SubModule.RequiredMsg"));

            RuleFor(r => r.TaskTypeId)
                .NotNull().WithMessageAwait(localizationService.GetResourceAsync("BacklogTaskModel.TaskType.RequiredMsg"));

            RuleFor(r => r.SeverityId)
                .NotNull().WithMessageAwait(localizationService.GetResourceAsync("BacklogTaskModel.Severity.RequiredMsg"));

            RuleFor(r => r.StatusId)
                .NotNull().WithMessageAwait(localizationService.GetResourceAsync("BacklogTaskModel.Status.RequiredMsg"));
        }
    }
}