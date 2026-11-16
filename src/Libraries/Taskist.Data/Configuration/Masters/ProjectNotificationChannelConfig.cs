using Taskist.Core.Domain.Masters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.Masters;

public class ProjectNotificationChannelConfig : EntityTypeConfiguration<ProjectNotificationChannel>
{
    public override void Configure(EntityTypeBuilder<ProjectNotificationChannel> builder)
    {
        builder.ToTable(nameof(ProjectNotificationChannel));

        builder.HasKey(x => x.Id);

        builder.HasOne(f => f.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(f => f.NotificationChannel)
            .WithMany()
            .HasForeignKey(p => p.NotificationChannelId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(p => new { p.ProjectId, p.NotificationChannelId })
            .IsUnique();

        base.Configure(builder);
    }
}
