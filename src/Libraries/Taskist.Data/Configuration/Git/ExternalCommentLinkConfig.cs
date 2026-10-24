using Taskist.Core.Domain.Git;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.Git;

public class ExternalCommentLinkConfig : EntityTypeConfiguration<ExternalCommentLink>
{
    public override void Configure(EntityTypeBuilder<ExternalCommentLink> builder)
    {
        builder.ToTable(nameof(ExternalCommentLink));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.ExternalCommentId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(p => p.ContentHash)
            .HasMaxLength(64);

        builder.HasOne(f => f.ExternalIssueLink)
            .WithMany()
            .HasForeignKey(p => p.ExternalIssueLinkId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(f => f.BacklogComment)
            .WithMany()
            .HasForeignKey(p => p.BacklogCommentId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(p => new { p.ExternalIssueLinkId, p.ExternalCommentId })
            .IsUnique();

        builder.HasIndex(p => p.BacklogCommentId);

        base.Configure(builder);
    }
}
