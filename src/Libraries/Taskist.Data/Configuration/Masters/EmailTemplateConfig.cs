using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Masters;

namespace Taskist.Data.Configuration.Masters;

public class EmailTemplateConfig : EntityTypeConfiguration<EmailTemplate>
{
    public override void Configure(EntityTypeBuilder<EmailTemplate> builder)
    {
        builder.ToTable(nameof(EmailTemplate));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        base.Configure(builder);
    }
}