using Taskist.Core.Domain.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.Logging;

public class AuditLogConfig : EntityTypeConfiguration<AuditLog>
{
    public override void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable(nameof(AuditLog));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.EntityName)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.Action)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.IpAddress)
            .HasMaxLength(100);

        builder.HasIndex(p => new { p.EntityName, p.EntityId });

        base.Configure(builder);
    }
}
