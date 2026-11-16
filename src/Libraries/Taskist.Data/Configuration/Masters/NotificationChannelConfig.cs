using Taskist.Core.Domain.Masters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.Masters;

public class NotificationChannelConfig : EntityTypeConfiguration<NotificationChannel>
{
    public override void Configure(EntityTypeBuilder<NotificationChannel> builder)
    {
        builder.ToTable(nameof(NotificationChannel));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.EndpointUrl)
            .IsRequired();

        builder.Property(p => p.LastError)
            .HasMaxLength(1000);

        base.Configure(builder);
    }
}
