using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Logging;

namespace Taskist.Data.Configuration.Logging;

public class ActivityLogConfig : EntityTypeConfiguration<ActivityLog>
{
    public override void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable(nameof(ActivityLog));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.SystemName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.EntityName)
            .HasMaxLength(500)
            .IsRequired();

        base.Configure(builder);
    }
}