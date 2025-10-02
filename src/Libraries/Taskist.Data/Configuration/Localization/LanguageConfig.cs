using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Localization;

namespace Taskist.Data.Configuration.Localization;

public class LanguageConfig : EntityTypeConfiguration<Language>
{
    public override void Configure(EntityTypeBuilder<Language> builder)
    {
        builder.ToTable(nameof(Language));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(750)
            .IsRequired();

        builder.Property(p => p.LanguageCulture)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(p => p.DisplayName)
            .HasMaxLength(750)
            .IsRequired();

        base.Configure(builder);
    }
}