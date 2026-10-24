using Taskist.Core.Domain.Git;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.Git;

public class GitWebhookDeliveryConfig : EntityTypeConfiguration<GitWebhookDelivery>
{
    public override void Configure(EntityTypeBuilder<GitWebhookDelivery> builder)
    {
        builder.ToTable(nameof(GitWebhookDelivery));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.DeliveryId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.EventName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Error)
            .HasMaxLength(1000);

        builder.HasOne(f => f.ProjectRepository)
            .WithMany()
            .HasForeignKey(p => p.ProjectRepositoryId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(p => new { p.ProjectRepositoryId, p.DeliveryId })
            .IsUnique();

        builder.HasIndex(p => p.ReceivedOn);

        base.Configure(builder);
    }
}
