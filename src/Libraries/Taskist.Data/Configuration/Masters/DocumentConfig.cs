using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Masters;

namespace Taskist.Data.Configuration.Masters;

public class DocumentConfig : EntityTypeConfiguration<Document>
{
    public override void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable(nameof(Document));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.FileName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.FileData)
            .IsRequired();

        builder.Property(p => p.ContentType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Extension)
           .HasMaxLength(10)
           .IsRequired();

        base.Configure(builder);
    }
}