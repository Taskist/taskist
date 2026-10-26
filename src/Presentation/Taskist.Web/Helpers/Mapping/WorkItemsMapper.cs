using Riok.Mapperly.Abstractions;
using Taskist.Core.Domain.WorkItems;
using Taskist.Web.Models.Masters;
using Taskist.Web.Models.WorkItems;

namespace Taskist.Web.Helpers.Mapping;

[Mapper]
public partial class WorkItemsMapper
{
    [MapperIgnoreTarget(nameof(BacklogModel.ParentTitle))]
    public partial BacklogModel ToModel(Backlog source);

    [MapperIgnoreTarget(nameof(Backlog.Code))]
    [MapProperty(nameof(BacklogModel.AssigneeId), nameof(Backlog.AssigneeId), Use = nameof(NullIfNegative))]
    [MapProperty(nameof(BacklogModel.ReleaseId), nameof(Backlog.ReleaseId), Use = nameof(NullIfNegative))]
    [MapProperty(nameof(BacklogModel.ReporterId), nameof(Backlog.ReporterId), Use = nameof(NullIfNegative))]
    [MapProperty(nameof(BacklogModel.TesterId), nameof(Backlog.TesterId), Use = nameof(NullIfNegative))]
    public partial Backlog ToEntity(BacklogModel source);

    [MapperIgnoreSource(nameof(Release.ReleaseProjects))]
    [MapperIgnoreTarget(nameof(ReleaseModel.AllProjects))]
    [MapperIgnoreTarget(nameof(ReleaseModel.SelectedProjectIds))]
    [MapperIgnoreTarget(nameof(ReleaseModel.ConfirmOpenTasks))]
    [MapperIgnoreTarget(nameof(ReleaseModel.CarryOverReleaseId))]
    [MapperIgnoreTarget(nameof(ReleaseModel.VersionAuto))]
    [MapperIgnoreTarget(nameof(ReleaseModel.VersionFormat))]
    [MapperIgnoreTarget(nameof(ReleaseModel.BaseReleaseId))]
    [MapperIgnoreTarget(nameof(ReleaseModel.AvailableBaseReleases))]
    public partial ReleaseModel ToModel(Release source);

    [MapperIgnoreTarget(nameof(Release.ReleaseProjects))]
    public partial Release ToEntity(ReleaseModel source);

    [MapperIgnoreTarget(nameof(Release.ReleaseProjects))]
    public partial void Update(ReleaseModel source, Release target);

    public partial CustomFieldValueModel ToModel(BacklogCustomFieldValue source);
    public partial BacklogCustomFieldValue ToEntity(CustomFieldValueModel source);

    private static int? NullIfNegative(int? value) => value == -1 ? null : value;
}
