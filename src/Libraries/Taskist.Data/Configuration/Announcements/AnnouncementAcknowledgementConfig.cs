using Taskist.Core.Domain.Announcements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.Announcements;

public class AnnouncementAcknowledgementConfig : EntityTypeConfiguration<AnnouncementAcknowledgement>
{
    public override void Configure(EntityTypeBuilder<AnnouncementAcknowledgement> builder)
    {
        builder.ToTable(nameof(AnnouncementAcknowledgement));

        builder.HasKey(x => x.Id);

        builder.HasOne(f => f.Announcement)
            .WithMany()
            .HasForeignKey(p => p.AnnouncementId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(p => new { p.AnnouncementId, p.UserId }).IsUnique();

        base.Configure(builder);
    }
}
