using Taskist.Core.Domain.WorkItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.WorkItems;

public class GitBranchTaskConfig : EntityTypeConfiguration<GitBranchTask>
{
    public override void Configure(EntityTypeBuilder<GitBranchTask> builder)
    {
        builder.ToTable(nameof(GitBranchTask));

        builder.HasKey(x => x.Id);

        builder.HasOne(f => f.GitBranch)
            .WithMany(b => b.Tasks)
            .HasForeignKey(p => p.GitBranchId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(f => f.Backlog)
            .WithMany()
            .HasForeignKey(p => p.BacklogId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(f => f.LinkedBy)
            .WithMany()
            .HasForeignKey(p => p.LinkedById)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(p => new { p.GitBranchId, p.BacklogId }).IsUnique();

        base.Configure(builder);
    }
}
