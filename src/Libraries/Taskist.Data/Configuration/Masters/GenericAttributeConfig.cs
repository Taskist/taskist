using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Masters;

namespace Taskist.Data.Configuration.Masters;

public class GenericAttributeConfig : EntityTypeConfiguration<GenericAttribute>
{
    public override void Configure(EntityTypeBuilder<GenericAttribute> builder)
    {
        builder.ToTable(nameof(GenericAttribute));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.KeyGroup)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Key)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Value)
            .IsRequired();

        base.Configure(builder);
    }
}