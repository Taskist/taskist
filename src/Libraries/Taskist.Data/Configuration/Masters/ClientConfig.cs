using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Masters;

namespace Taskist.Data.Configuration.Masters;

public class ClientConfig : EntityTypeConfiguration<Client>
{
    public override void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable(nameof(Client));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(250);

        builder.Property(p => p.ContactPerson)
            .HasMaxLength(100);

        builder.Property(p => p.PhoneNumber)
            .HasMaxLength(100);

        builder.Property(p => p.Email)
            .HasMaxLength(250);

        builder.Property(p => p.Website)
            .HasMaxLength(750);

        base.Configure(builder);
    }
}