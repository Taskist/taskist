using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Masters;

namespace Taskist.Data.Configuration.Masters;

public class MenuConfig : EntityTypeConfiguration<Menu>
{
    public override void Configure(EntityTypeBuilder<Menu> builder)
    {
        builder.ToTable(nameof(Menu));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.SystemName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.ActionName)
            .HasMaxLength(100);

        builder.Property(p => p.ControllerName)
            .HasMaxLength(100);

        builder.Property(p => p.Icon)
            .HasMaxLength(100);

        builder.Property(p => p.Permission)
            .HasMaxLength(100)
            .IsRequired();

        base.Configure(builder);
    }
}