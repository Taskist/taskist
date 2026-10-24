using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Taskist.Tests.Infrastructure;

public class TestWebHostEnvironment : IWebHostEnvironment
{
    #region Properties

    public string WebRootPath { get; set; } = Path.GetTempPath();

    public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

    public string ApplicationName { get; set; } = "Taskist.Tests";

    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

    public string ContentRootPath { get; set; } = Path.GetTempPath();

    public string EnvironmentName { get; set; } = "Production";

    #endregion
}
