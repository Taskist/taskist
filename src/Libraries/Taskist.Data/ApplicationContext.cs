using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Taskist.Data.Configuration;

namespace Taskist.Data;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var typeConfigurations = Assembly.GetExecutingAssembly().GetTypes().Where(type =>
            (type.BaseType?.IsGenericType ?? false)
                && (type.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>)));

        foreach (var typeConfiguration in typeConfigurations)
        {
            var configuration = (IMappingConfiguration)Activator.CreateInstance(typeConfiguration);
            configuration.ApplyConfiguration(modelBuilder);
        }

        base.OnModelCreating(modelBuilder);
    }
}