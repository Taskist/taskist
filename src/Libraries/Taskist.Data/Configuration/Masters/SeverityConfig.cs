using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Masters;

namespace Taskist.Data.Configuration.Masters;

public class SeverityConfig : EntityTypeConfiguration<Severity>
{
    public override void Configure(EntityTypeBuilder<Severity> builder)
    {
        builder.ToTable(nameof(Severity));

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

        base.Configure(builder);
    }
}