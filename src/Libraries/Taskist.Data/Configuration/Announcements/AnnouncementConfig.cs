using Taskist.Core.Domain.Announcements;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.Announcements;

public class AnnouncementConfig : EntityTypeConfiguration<Announcement>
{
    public override void Configure(EntityTypeBuilder<Announcement> builder)
    {
        builder.ToTable(nameof(Announcement));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Body)
            .IsRequired();

        builder.HasIndex(p => new { p.Active, p.Deleted });

        base.Configure(builder);
    }
}
