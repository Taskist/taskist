using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Masters;

namespace Taskist.Data.Configuration.Masters;

public class ReporterConfig : EntityTypeConfiguration<Reporter>
{
    public override void Configure(EntityTypeBuilder<Reporter> builder)
    {
        builder.ToTable(nameof(Reporter));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(250);

        builder.HasOne(f => f.Project)
                .WithMany()
                .HasForeignKey(p => p.ProjectId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

        base.Configure(builder);
    }
}