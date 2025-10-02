using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Localization;

namespace Taskist.Data.Configuration.Localization;

public class LocaleResourceConfig : EntityTypeConfiguration<LocaleResource>
{
    public override void Configure(EntityTypeBuilder<LocaleResource> builder)
    {
        builder.ToTable(nameof(LocaleResource));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.ResourceKey)
           .HasMaxLength(100)
           .IsRequired();

        builder.HasOne(f => f.Language)
            .WithMany()
            .HasForeignKey(p => p.LanguageId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        base.Configure(builder);
    }
}