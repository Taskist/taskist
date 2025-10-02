using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Masters;

namespace Taskist.Data.Configuration.Masters;

public class EmailAccountConfig : EntityTypeConfiguration<EmailAccount>
{
    public override void Configure(EntityTypeBuilder<EmailAccount> builder)
    {
        builder.ToTable(nameof(EmailAccount));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(250);

        base.Configure(builder);
    }
}