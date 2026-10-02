using Mapster;
using Mapster.Models;
using Taskist.Core.Domain.Common;
using Taskist.Core.Domain.Localization;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.WorkItems;
using Menu = Taskist.Core.Domain.Masters.Menu;
using Severity = Taskist.Core.Domain.Masters.Severity;
using Taskist.Web.Models.Localization;
using Taskist.Web.Models.Masters;
using Taskist.Web.Models.Users;
using Taskist.Web.Models.WorkItems;

namespace Taskist.Web.Helpers.Mapping;

/// <summary>
/// Mapster mapping configuration (replaces the former AutoMapper profile).
/// TwoWays() mirrors AutoMapper's ReverseMap(); customised reverse directions
/// are declared as their own NewConfig so ignores / value maps apply to the
/// correct direction, exactly as the AutoMapper ForMember chains did.
/// </summary>
public class MappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Never map INTO an EF navigation property (Client, TaskType, Project, ...).
        // Mapster would otherwise "unflatten" a scalar FK (e.g. ClientId) into a new
        // nav object (Client { Id = ... }); EF then treats it as new and tries an
        // identity insert. Only the scalar FK columns are mapped; reads (entity -> model,
        // including flattened fields like ClientName) are unaffected.
        config.Default.IgnoreMember((member, side) =>
            side == MemberSide.Destination && typeof(BaseEntity).IsAssignableFrom(member.Type));

        #region Masters

        config.NewConfig<Reporter, ReporterModel>().TwoWays();
        config.NewConfig<EmailAccount, EmailAccountModel>().TwoWays();
        config.NewConfig<EmailTemplate, EmailTemplateModel>().TwoWays();
        config.NewConfig<Severity, SeverityModel>().TwoWays();
        config.NewConfig<Status, StatusModel>().TwoWays();
        config.NewConfig<TaskType, TaskTypeModel>().TwoWays();
        config.NewConfig<Menu, MenuItemModel>().TwoWays();
        config.NewConfig<Client, ClientModel>().TwoWays();
        config.NewConfig<Project, ProjectModel>().TwoWays();
        config.NewConfig<Project, ProjectGridModel>().TwoWays();
        config.NewConfig<UserProjectMap, UserProjectModel>().TwoWays();
        config.NewConfig<Module, ModuleModel>().TwoWays();

        config.NewConfig<CustomField, CustomFieldModel>();
        config.NewConfig<CustomFieldModel, CustomField>()
            .Ignore(dest => dest.Project);

        config.NewConfig<SubModule, SubModuleModel>();
        config.NewConfig<SubModuleModel, SubModule>()
            .Ignore(dest => dest.Module);

        #endregion

        #region Localization

        config.NewConfig<Language, LanguageModel>().TwoWays();
        config.NewConfig<LocaleResource, LocaleResourceModel>().TwoWays();

        #endregion

        #region Users

        config.NewConfig<User, UserModel>();
        config.NewConfig<UserModel, User>()
            .Ignore(dest => dest.Code);
        config.NewConfig<User, ProfileDetailModel>().TwoWays();
        config.NewConfig<UserRole, UserRoleModel>().TwoWays();
        config.NewConfig<UserRolePermission, UserRolePermissionModel>().TwoWays();
        config.NewConfig<UserRolePermissionMap, UserRolePermissionMapModel>().TwoWays();

        #endregion

        #region Work Items

        config.NewConfig<Backlog, BacklogModel>();
        config.NewConfig<BacklogModel, Backlog>()
            .Ignore(dest => dest.Code)
            .Map(dest => dest.AssigneeId, src => src.AssigneeId == -1 ? (int?)null : src.AssigneeId)
            .Map(dest => dest.SprintId, src => src.SprintId == -1 ? (int?)null : src.SprintId)
            .Map(dest => dest.ReporterId, src => src.ReporterId == -1 ? (int?)null : src.ReporterId);

        config.NewConfig<Sprint, SprintModel>().TwoWays();
        config.NewConfig<CustomField, CustomFieldRenderModel>();
        config.NewConfig<BacklogCustomFieldValue, CustomFieldValueModel>().TwoWays();

        #endregion
    }
}
