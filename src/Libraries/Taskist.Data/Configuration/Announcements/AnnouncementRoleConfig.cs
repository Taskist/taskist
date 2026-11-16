using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Announcements;

namespace Taskist.Data.Configuration.Announcements;

public class AnnouncementRoleConfig : EntityTypeConfiguration<AnnouncementRole>
{
    public override void Configure(EntityTypeBuilder<AnnouncementRole> builder)
    {
        builder.ToTable(nameof(AnnouncementRole));

        builder.HasKey(x => x.Id);

        builder.HasOne(f => f.Announcement)
            .WithMany(a => a.Roles)
            .HasForeignKey(p => p.AnnouncementId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(f => f.UserRole)
            .WithMany()
            .HasForeignKey(p => p.UserRoleId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(p => new { p.AnnouncementId, p.UserRoleId });

        base.Configure(builder);
    }
}
