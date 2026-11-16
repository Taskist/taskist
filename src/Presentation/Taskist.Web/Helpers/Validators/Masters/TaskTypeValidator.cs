using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Masters;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.Masters;

public class TaskTypeValidator : AbstractValidator<TaskTypeModel>
{
    public TaskTypeValidator(ILocalizationService localizationService,
        ITaskTypeService severityService)
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage(localizationService.GetResource("TaskTypeModel.Name.RequiredMsg"))
            .MaximumLength(100).WithMessage(localizationService.GetResource("TaskTypeModel.Name.MaxLengthMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await severityService.GetByNameAsync(x.Name);
                    return editedEntity == null || editedEntity.Id == x.Id;
                }
                var entity = await severityService.GetByNameAsync(x.Name);
                return entity == null;
            }).WithMessage(localizationService.GetResource("TaskTypeModel.Name.UniqueMsg"));

        RuleFor(r => r.Description)
            .MaximumLength(250).WithMessage(localizationService.GetResource("TaskTypeModel.Description.MaxLengthMsg"));

        RuleFor(r => r.GroupId)
           .GreaterThan(0).WithMessage(localizationService.GetResource("TaskTypeModel.Group.RequiredMsg"));

        RuleFor(r => r.TextColor)
            .MaximumLength(20).WithMessage(localizationService.GetResource("TaskTypeModel.TextColor.MaxLengthMsg"));

        RuleFor(r => r.BackgroundColor)
            .MaximumLength(20).WithMessage(localizationService.GetResource("TaskTypeModel.BackgroundColor.MaxLengthMsg"));

        RuleFor(r => r.IconClass)
            .MaximumLength(50).WithMessage(localizationService.GetResource("TaskTypeModel.IconClass.MaxLengthMsg"));
    }
}