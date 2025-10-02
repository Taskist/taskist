using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.WorkItems;

namespace Taskist.Data.Configuration.WorkItems;

public class BacklogStatusLogConfig : EntityTypeConfiguration<BacklogStatusLog>
{
    public override void Configure(EntityTypeBuilder<BacklogStatusLog> builder)
    {
        builder.ToTable(nameof(BacklogStatusLog));

        builder.HasKey(x => x.Id);

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

        builder.HasOne(f => f.Status)
            .WithMany()
            .HasForeignKey(p => p.StatusId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        base.Configure(builder);
    }
}