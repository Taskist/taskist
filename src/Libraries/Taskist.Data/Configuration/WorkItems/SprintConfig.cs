using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.WorkItems;

namespace Taskist.Data.Configuration.WorkItems;

public class SprintConfig : EntityTypeConfiguration<Sprint>
{
    public override void Configure(EntityTypeBuilder<Sprint> builder)
    {
        builder.ToTable(nameof(Sprint));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.HasOne(f => f.Project)
                .WithMany()
                .HasForeignKey(p => p.ProjectId)
                .OnDelete(DeleteBehavior.NoAction)
                .IsRequired(true);

        base.Configure(builder);
    }
}