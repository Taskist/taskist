namespace Taskist.Web.Infrastructure;

public static class AssetExtensions
{
    public static IServiceCollection AddAssetPipeline(this IServiceCollection services)
    {
        services.AddWebOptimizer(pipeline =>
        {
            pipeline.AddCssBundle("/css/bundle.min.css", "css/icons.css", "css/site.css", "css/appearance.css", "css/theme-dark.generated.css", "css/theme-dark.css");

            pipeline.AddJavaScriptBundle("/js/bundle.min.js",
                            "js/datatable-helper.js", "js/common.js", "js/app.js");
        });

        return services;
    }
}
