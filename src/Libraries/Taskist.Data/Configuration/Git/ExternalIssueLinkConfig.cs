using Taskist.Core.Domain.Git;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.Git;

public class ExternalIssueLinkConfig : EntityTypeConfiguration<ExternalIssueLink>
{
    public override void Configure(EntityTypeBuilder<ExternalIssueLink> builder)
    {
        builder.ToTable(nameof(ExternalIssueLink));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.ExternalId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(p => p.Url)
            .HasMaxLength(500);

        builder.Property(p => p.RemoteState)
            .HasMaxLength(20);

        builder.Property(p => p.LastError)
            .HasMaxLength(1000);

        builder.HasOne(f => f.Backlog)
            .WithMany()
            .HasForeignKey(p => p.BacklogId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(f => f.ProjectRepository)
            .WithMany()
            .HasForeignKey(p => p.ProjectRepositoryId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(p => new { p.ProjectRepositoryId, p.Number })
            .IsUnique();

        builder.HasIndex(p => p.BacklogId);

        base.Configure(builder);
    }
}
