using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Masters;

namespace Taskist.Data.Configuration.Masters;

public class LabelConfig : EntityTypeConfiguration<Label>
{
    public override void Configure(EntityTypeBuilder<Label> builder)
    {
        builder.ToTable(nameof(Label));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(250);

        builder.Property(p => p.Color)
            .HasMaxLength(20);

        builder.Property(x => x.Deleted)
              .HasDefaultValue(false);
        builder.Property(x => x.Active)
              .HasDefaultValue(false);

        base.Configure(builder);
    }
}