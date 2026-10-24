using Taskist.Core.Domain.WorkItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.WorkItems;

public class ReleaseProjectConfig : EntityTypeConfiguration<ReleaseProject>
{
    public override void Configure(EntityTypeBuilder<ReleaseProject> builder)
    {
        builder.ToTable(nameof(ReleaseProject));

        builder.HasKey(x => x.Id);

        builder.HasOne(f => f.Release)
            .WithMany(r => r.ReleaseProjects)
            .HasForeignKey(p => p.ReleaseId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(f => f.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(p => new { p.ReleaseId, p.ProjectId }).IsUnique();

        base.Configure(builder);
    }
}
