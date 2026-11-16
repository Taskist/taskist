using Taskist.Core.Domain.WorkItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.WorkItems;

public class BacklogCommentConfig : EntityTypeConfiguration<BacklogComment>
{
    public override void Configure(EntityTypeBuilder<BacklogComment> builder)
    {
        builder.ToTable(nameof(BacklogComment));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Comment)
            .IsRequired();

        builder.Property(p => p.FieldName)
            .HasMaxLength(100);

        builder.HasOne(f => f.Backlog)
            .WithMany()
            .HasForeignKey(p => p.BacklogId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        builder.HasOne(f => f.CreatedBy)
            .WithMany()
            .HasForeignKey(p => p.CreatedById)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        builder.HasOne(f => f.ModifiedBy)
            .WithMany()
            .HasForeignKey(p => p.ModifiedById)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        base.Configure(builder);
    }
}