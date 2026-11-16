using Taskist.Core.Domain.WorkItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.WorkItems;

public class ReleaseConfig : EntityTypeConfiguration<Release>
{
    public override void Configure(EntityTypeBuilder<Release> builder)
    {
        builder.ToTable(nameof(Release));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.HasOne(f => f.Project)
                .WithMany()
                .HasForeignKey(p => p.ProjectId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(false);

        base.Configure(builder);
    }
}