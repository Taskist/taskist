using Taskist.Core.Domain.Masters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.Masters;

public class AddressConfig : EntityTypeConfiguration<Address>
{
    public override void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable(nameof(Address));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Address1)
            .HasMaxLength(750)
            .IsRequired();

        builder.Property(p => p.Address2)
            .HasMaxLength(750);

        builder.Property(p => p.City)
            .HasMaxLength(100);

        builder.Property(p => p.ZipPostalCode)
            .HasMaxLength(20);

        base.Configure(builder);
    }
}