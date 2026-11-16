using Taskist.Core.Domain.WorkItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.WorkItems;

public class StatusDurationMetricConfig : EntityTypeConfiguration<StatusDurationMetric>
{
    public override void Configure(EntityTypeBuilder<StatusDurationMetric> builder)
    {
        builder.ToTable(nameof(StatusDurationMetric));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasOne(f => f.FromStatus)
            .WithMany()
            .HasForeignKey(p => p.FromStatusId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        builder.HasOne(f => f.ToStatus)
            .WithMany()
            .HasForeignKey(p => p.ToStatusId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        builder.HasOne(f => f.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        base.Configure(builder);
    }
}
