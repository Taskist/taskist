using Taskist.Core.Domain.Git;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.Git;

public class ProjectRepositoryConfig : EntityTypeConfiguration<ProjectRepository>
{
    public override void Configure(EntityTypeBuilder<ProjectRepository> builder)
    {
        builder.ToTable(nameof(ProjectRepository));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.RepoRef)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.ExternalRepoId)
            .HasMaxLength(64);

        builder.Property(p => p.DisplayName)
            .HasMaxLength(255);

        builder.Property(p => p.WebUrl)
            .HasMaxLength(500);

        builder.Property(p => p.DefaultBranch)
            .HasMaxLength(255);

        builder.Property(p => p.WebhookKey)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(p => p.IssueImportLabel)
            .HasMaxLength(100);

        builder.Property(p => p.IssueSyncTaskTypeIds)
            .HasMaxLength(200);

        builder.HasOne(f => f.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(f => f.GitConnection)
            .WithMany()
            .HasForeignKey(p => p.GitConnectionId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(f => f.IssueTaskType)
            .WithMany()
            .HasForeignKey(p => p.IssueTaskTypeId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(f => f.IssueSeverity)
            .WithMany()
            .HasForeignKey(p => p.IssueSeverityId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(f => f.IssueOpenStatus)
            .WithMany()
            .HasForeignKey(p => p.IssueOpenStatusId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(f => f.IssueClosedStatus)
            .WithMany()
            .HasForeignKey(p => p.IssueClosedStatusId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(f => f.IssueSyncUser)
            .WithMany()
            .HasForeignKey(p => p.IssueSyncUserId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasIndex(p => p.WebhookKey)
            .IsUnique();

        builder.HasIndex(p => new { p.ProjectId, p.GitConnectionId, p.RepoRef });

        base.Configure(builder);
    }
}
