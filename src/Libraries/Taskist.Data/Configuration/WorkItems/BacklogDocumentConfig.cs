using Taskist.Core.Domain.WorkItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.WorkItems;

public class BacklogDocumentConfig : EntityTypeConfiguration<BacklogDocument>
{
    public override void Configure(EntityTypeBuilder<BacklogDocument> builder)
    {
        builder.ToTable(nameof(BacklogDocument));

        builder.HasKey(x => x.Id);

        builder.HasOne(f => f.Backlog)
            .WithMany()
            .HasForeignKey(p => p.BacklogId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        builder.HasOne(f => f.Document)
            .WithMany()
            .HasForeignKey(p => p.DocumentId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        builder.HasOne(f => f.CreatedBy)
            .WithMany()
            .HasForeignKey(p => p.CreatedById)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        base.Configure(builder);
    }
}