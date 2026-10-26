using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Masters;
using Taskist.Core.Domain.Users;
using Taskist.Core.Domain.Wiki;

namespace Taskist.Data.Configuration.Wiki;

public class WikiImageConfig : EntityTypeConfiguration<WikiImage>
{
    public override void Configure(EntityTypeBuilder<WikiImage> builder)
    {
        builder.ToTable(nameof(WikiImage));

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.DocumentId)
            .IsUnique();

        builder.HasOne<Document>()
            .WithMany()
            .HasForeignKey(x => x.DocumentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<Project>()
            .WithMany()
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<WikiDocument>()
            .WithMany()
            .HasForeignKey(x => x.WikiDocumentId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UploadedById)
            .OnDelete(DeleteBehavior.NoAction);

        base.Configure(builder);
    }
}
