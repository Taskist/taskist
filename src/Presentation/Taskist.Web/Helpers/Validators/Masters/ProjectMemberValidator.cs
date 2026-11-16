using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Users;
using FluentValidation;

namespace Taskist.Web.Helpers.Validators.Masters;

public class ProjectMemberValidator : AbstractValidator<UserProjectModel>
{
    public ProjectMemberValidator(ILocalizationService localizationService,
        IProjectService projectService)
    {
        RuleFor(r => r.UserId)
            .NotEmpty().WithMessage(localizationService.GetResource("ProjectMemberModel.User.RequiredMsg"))
            .GreaterThan(0).WithMessage(localizationService.GetResource("ProjectMemberModel.User.RequiredMsg"))
            .MustAwait(async (x, context) =>
            {
                if (x.Id > 0)
                {
                    var editedEntity = await projectService.GetMemberByIdAndProjectAsync(x.UserId, x.ProjectId);
                    return (editedEntity != null) && (editedEntity.UserId == x.UserId);
                }
                var entity = await projectService.GetMemberByIdAndProjectAsync(x.UserId, x.ProjectId);
                return entity == null;
                return entity == null;
            }).WithMessage(localizationService.GetResource("ProjectMemberModel.User.UniqueMsg"));
    }
}