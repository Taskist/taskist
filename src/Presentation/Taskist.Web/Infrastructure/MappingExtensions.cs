using Taskist.Web.Helpers.Mapping;
namespace Taskist.Web.Infrastructure;

public static class MappingExtensions
{
    public static IServiceCollection AddObjectMapping(this IServiceCollection services)
    {
        services.AddSingleton<MastersMapper>();
        services.AddSingleton<UsersMapper>();
        services.AddSingleton<WorkItemsMapper>();
        services.AddSingleton<LocalizationMapper>();
        services.AddSingleton<AnnouncementMapper>();

        return services;
    }
}
