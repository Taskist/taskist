using Taskist.Core.Domain.WorkItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.WorkItems;

public class BacklogTagConfig : EntityTypeConfiguration<BacklogTag>
{
    public override void Configure(EntityTypeBuilder<BacklogTag> builder)
    {
        builder.ToTable(nameof(BacklogTag));

        builder.HasKey(x => x.Id);

        builder.HasOne(f => f.Backlog)
            .WithMany()
            .HasForeignKey(p => p.BacklogId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(f => f.Tag)
            .WithMany()
            .HasForeignKey(p => p.TagId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(p => new { p.BacklogId, p.TagId });

        base.Configure(builder);
    }
}
