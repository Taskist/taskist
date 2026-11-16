namespace Taskist.Web.Helpers.Appearance;

public class AppearanceSettings
{
    public string Mode { get; set; } = AppearanceOptions.DefaultMode;

    public string Accent { get; set; } = AppearanceOptions.DefaultAccent;

    public string RenderedTheme => Mode == AppearanceOptions.Dark ? AppearanceOptions.Dark : AppearanceOptions.Light;
}
