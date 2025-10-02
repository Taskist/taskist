using FluentValidation;
using Taskist.Service.Localization;
using Taskist.Service.Masters;
using Taskist.Web.Helpers.Extensions;
using Taskist.Web.Models.Users;

namespace Taskist.Web.Helpers.Validators.Masters;

public class ProjectMemberValidator : AbstractValidator<UserProjectModel>
{
    public ProjectMemberValidator(ILocalizationService localizationService,
        IProjectService projectService)
    {
        RuleFor(r => r.UserId)
            .NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("ProjectMemberModel.User.RequiredMsg"))
            .GreaterThan(0).WithMessageAwait(localizationService.GetResourceAsync("ProjectMemberModel.User.RequiredMsg"))
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
            }).WithMessageAwait(localizationService.GetResourceAsync("ProjectMemberModel.User.UniqueMsg"));
    }
}