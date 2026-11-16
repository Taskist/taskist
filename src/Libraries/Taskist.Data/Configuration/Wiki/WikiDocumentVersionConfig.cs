using Taskist.Core.Domain.Wiki;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.Wiki;

public class WikiDocumentVersionConfig : EntityTypeConfiguration<WikiDocumentVersion>
{
    public override void Configure(EntityTypeBuilder<WikiDocumentVersion> builder)
    {
        builder.ToTable(nameof(WikiDocumentVersion));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Title)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(p => p.ContentHtml)
            .IsRequired();

        builder.HasOne(f => f.WikiDocument)
            .WithMany()
            .HasForeignKey(p => p.WikiDocumentId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        builder.HasOne(f => f.ModifiedBy)
            .WithMany()
            .HasForeignKey(p => p.ModifiedById)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        base.Configure(builder);
    }
}