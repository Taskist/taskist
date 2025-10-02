using AutoMapper;
using Taskist.Core.Domain.Localization;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.WorkItems;
using Taskist.Web.Models.Localization;
using Taskist.Web.Models.Masters;
using Taskist.Web.Models.Users;
using Taskist.Web.Models.WorkItems;

namespace Taskist.Web.Helpers.Mapping
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            #region Masters

            CreateMap<Reporter, ReporterModel>().ReverseMap();
            CreateMap<EmailAccount, EmailAccountModel>().ReverseMap();
            CreateMap<EmailTemplate, EmailTemplateModel>().ReverseMap();
            CreateMap<Severity, SeverityModel>().ReverseMap();
            CreateMap<Status, StatusModel>().ReverseMap();
            CreateMap<TaskType, TaskTypeModel>().ReverseMap();
            CreateMap<Menu, MenuItemModel>().ReverseMap();
            CreateMap<Client, ClientModel>().ReverseMap();
            CreateMap<Project, ProjectModel>().ReverseMap();
            CreateMap<Project, ProjectGridModel>().ReverseMap();
            CreateMap<Project, ProjectGridModel>().ReverseMap();
            CreateMap<UserProjectMap, UserProjectModel>().ReverseMap();
            CreateMap<CustomField, CustomFieldModel>().ReverseMap()
                .ForMember(dest => dest.Project, act => act.Ignore());
            CreateMap<Module, ModuleModel>().ReverseMap();
            CreateMap<SubModule, SubModuleModel>();
            CreateMap<SubModuleModel, SubModule>()
                .ForMember(dest => dest.Module, act => act.Ignore());

            #endregion

            #region Localization

            CreateMap<Language, LanguageModel>().ReverseMap();
            CreateMap<LocaleResource, LocaleResourceModel>().ReverseMap();

            #endregion

            #region Users

            CreateMap<User, UserModel>().ReverseMap()
                .ForMember(dest => dest.Code, act => act.Ignore());
            CreateMap<UserRole, UserRoleModel>().ReverseMap();
            CreateMap<UserRolePermission, UserRolePermissionModel>().ReverseMap();
            CreateMap<UserRolePermissionMap, UserRolePermissionMapModel>().ReverseMap();

            #endregion

            #region Work Items

            CreateMap<Backlog, BacklogModel>().ReverseMap()
                .ForMember(dest => dest.Code, act => act.Ignore())
                .ForMember(dest => dest.AssigneeId, opt => opt.MapFrom(src => src.AssigneeId == -1 ? (int?)null : src.AssigneeId));

            CreateMap<Sprint, SprintModel>().ReverseMap();
            CreateMap<CustomField, CustomFieldRenderModel>();
            CreateMap<BacklogCustomFieldValue, CustomFieldValueModel>().ReverseMap();

            #endregion
        }
    }
}
