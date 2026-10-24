using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Users;

namespace Taskist.Data.Configuration.Users;

public class UserNotificationConfig : EntityTypeConfiguration<UserNotification>
{
    public override void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.ToTable(nameof(UserNotification));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Title)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(p => p.Message)
            .HasMaxLength(1000);

        builder.Property(p => p.Url)
            .HasMaxLength(500);

        builder.Property(p => p.DedupeKey)
            .HasMaxLength(200);

        builder.HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(p => new { p.UserId, p.IsRead, p.CreatedOnUtc });

        builder.HasIndex(p => new { p.UserId, p.DedupeKey });

        base.Configure(builder);
    }
}
