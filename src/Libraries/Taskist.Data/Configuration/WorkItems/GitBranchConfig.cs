using Taskist.Core.Domain.WorkItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.WorkItems;

public class GitBranchConfig : EntityTypeConfiguration<GitBranch>
{
    public override void Configure(EntityTypeBuilder<GitBranch> builder)
    {
        builder.ToTable(nameof(GitBranch));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.GitProjectReference)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.Name)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.BaseBranch)
            .HasMaxLength(255);

        builder.HasOne(f => f.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(f => f.CreatedBy)
            .WithMany()
            .HasForeignKey(p => p.CreatedById)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(f => f.ProjectRepository)
            .WithMany()
            .HasForeignKey(p => p.ProjectRepositoryId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasIndex(p => new { p.GitProjectReference, p.Name });

        base.Configure(builder);
    }
}
