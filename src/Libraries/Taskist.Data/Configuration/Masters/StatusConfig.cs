using Taskist.Core.Domain.Masters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.Masters;

public class StatusConfig : EntityTypeConfiguration<Status>
{
    public override void Configure(EntityTypeBuilder<Status> builder)
    {
        builder.ToTable(nameof(Status));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(250);

        builder.Property(p => p.TextColor)
            .HasMaxLength(20);

        builder.Property(p => p.BackgroundColor)
            .HasMaxLength(20);

        builder.Property(p => p.IconClass)
            .HasMaxLength(50);

        builder.HasOne(f => f.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        base.Configure(builder);
    }
}