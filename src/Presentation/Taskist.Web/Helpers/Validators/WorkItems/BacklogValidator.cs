using Taskist.Service.Localization;
using Taskist.Web.Models.WorkItems;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.WorkItems
{
    public class BacklogValidator : AbstractValidator<BacklogModel>
    {
        public BacklogValidator(ILocalizationService localizationService)
        {
            RuleFor(r => r.Title)
                .NotEmpty().WithMessage(localizationService.GetResource("BacklogModel.Title.RequiredMsg"))
                .MaximumLength(500).WithMessage(localizationService.GetResource("BacklogModel.Title.MaxLengthMsg"));

            RuleFor(r => r.ModuleId)
                .NotEmpty().WithMessage(localizationService.GetResource("BacklogModel.Module.RequiredMsg"));

            RuleFor(r => r.SubModuleId)
               .NotEmpty().WithMessage(localizationService.GetResource("BacklogModel.SubModule.RequiredMsg"));

            RuleFor(r => r.TaskTypeId)
                .NotNull().WithMessage(localizationService.GetResource("BacklogModel.TaskType.RequiredMsg"));

            RuleFor(r => r.SeverityId)
                .NotNull().WithMessage(localizationService.GetResource("BacklogModel.Severity.RequiredMsg"));

            RuleFor(r => r.ReporterId)
                .NotNull().WithMessage(localizationService.GetResource("BacklogModel.Reporter.RequiredMsg"));

            RuleFor(r => r.StatusId)
                .NotNull().WithMessage(localizationService.GetResource("BacklogModel.Status.RequiredMsg"));
        }
    }
}
