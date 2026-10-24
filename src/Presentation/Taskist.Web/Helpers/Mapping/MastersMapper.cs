using Riok.Mapperly.Abstractions;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.WorkItems;
using Taskist.Web.Models.Masters;
using Taskist.Web.Models.WorkItems;

namespace Taskist.Web.Helpers.Mapping;

[Mapper]
public partial class MastersMapper
{
    public partial ReporterModel ToModel(Reporter source);
    public partial Reporter ToEntity(ReporterModel source);
    public partial void Update(ReporterModel source, Reporter target);

    public partial EmailAccountModel ToModel(EmailAccount source);
    public partial EmailAccount ToEntity(EmailAccountModel source);
    public partial void Update(EmailAccountModel source, EmailAccount target);

    public partial EmailTemplateModel ToModel(EmailTemplate source);
    public partial EmailTemplate ToEntity(EmailTemplateModel source);
    public partial void Update(EmailTemplateModel source, EmailTemplate target);

    public partial SeverityModel ToModel(Severity source);
    public partial Severity ToEntity(SeverityModel source);
    public partial void Update(SeverityModel source, Severity target);

    public partial StatusModel ToModel(Status source);
    public partial Status ToEntity(StatusModel source);
    public partial void Update(StatusModel source, Status target);

    public partial TaskTypeModel ToModel(TaskType source);
    public partial TaskType ToEntity(TaskTypeModel source);
    public partial void Update(TaskTypeModel source, TaskType target);

    public partial AddressModel ToModel(Address source);
    public partial Address ToEntity(AddressModel source);
    public partial void Update(AddressModel source, Address target);

    public partial MenuItemModel ToModel(Menu source);

    public partial ClientModel ToModel(Client source);
    public partial Client ToEntity(ClientModel source);
    public partial void Update(ClientModel source, Client target);

    public partial ProjectModel ToModel(Project source);
    public partial ProjectGridModel ToGridModel(Project source);
    public partial Project ToEntity(ProjectModel source);
    public partial void Update(ProjectModel source, Project target);

    public partial ModuleModel ToModel(Module source);
    public partial Module ToEntity(ModuleModel source);
    public partial void Update(ModuleModel source, Module target);

    public partial SubModuleModel ToModel(SubModule source);

    [MapperIgnoreTarget(nameof(SubModule.Module))]
    public partial SubModule ToEntity(SubModuleModel source);

    [MapperIgnoreTarget(nameof(SubModule.Module))]
    public partial void Update(SubModuleModel source, SubModule target);

    public partial CustomFieldModel ToModel(CustomField source);

    [MapperIgnoreTarget(nameof(CustomField.Project))]
    public partial CustomField ToEntity(CustomFieldModel source);

    [MapperIgnoreTarget(nameof(CustomField.Project))]
    public partial void Update(CustomFieldModel source, CustomField target);

    public partial CustomFieldRenderModel ToRenderModel(CustomField source);

    public partial TagModel ToModel(Tag source);
    public partial Tag ToEntity(TagModel source);
    public partial void Update(TagModel source, Tag target);
}
