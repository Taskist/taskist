using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Taskist.Data;

public class ApplicationContextFactory : IDesignTimeDbContextFactory<ApplicationContext>
{
	#region Methods

	public ApplicationContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();
		optionsBuilder.UseSqlServer(ResolveConnectionString());

		return new ApplicationContext(optionsBuilder.Options, null!);
	}

	#endregion

	#region Utilities

	private static string ResolveConnectionString()
	{
		foreach (var basePath in CandidateWebPaths())
		{
			if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
				continue;

			var configuration = new ConfigurationBuilder()
				.SetBasePath(basePath)
				.AddJsonFile("appsettings.json", optional: false)
				.AddJsonFile("appsettings.Development.json", optional: true)
				.Build();

			var connectionString = configuration.GetConnectionString("AppContext");
			if (!string.IsNullOrWhiteSpace(connectionString))
				return connectionString;
		}

		return "Server=localhost;Database=Taskist;Trusted_Connection=True;TrustServerCertificate=True;";
	}

	private static IEnumerable<string> CandidateWebPaths()
	{
		var current = Directory.GetCurrentDirectory();

		yield return Path.GetFullPath(Path.Combine(current, "..", "..", "Presentation", "Taskist.Web"));

		var dir = new DirectoryInfo(current);
		while (dir != null)
		{
			var candidate = Path.Combine(dir.FullName, "src", "Presentation", "Taskist.Web");
			if (Directory.Exists(candidate))
				yield return candidate;

			dir = dir.Parent;
		}
	}

	#endregion
}
