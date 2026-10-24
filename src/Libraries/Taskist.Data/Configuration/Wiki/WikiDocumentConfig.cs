using Taskist.Core.Domain.Wiki;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.Wiki;

public class WikiDocumentConfig : EntityTypeConfiguration<WikiDocument>
{
    public override void Configure(EntityTypeBuilder<WikiDocument> builder)
    {
        builder.ToTable(nameof(WikiDocument));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Title)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(p => p.ContentHtml)
            .IsRequired();

        builder.HasOne(f => f.CreatedBy)
            .WithMany()
            .HasForeignKey(p => p.CreatedById)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        builder.HasOne(f => f.ModifiedBy)
            .WithMany()
            .HasForeignKey(p => p.ModifiedById)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        builder.HasOne(f => f.Module)
            .WithMany()
            .HasForeignKey(p => p.ModuleId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(f => f.SubModule)
            .WithMany()
            .HasForeignKey(p => p.SubModuleId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        base.Configure(builder);
    }
}