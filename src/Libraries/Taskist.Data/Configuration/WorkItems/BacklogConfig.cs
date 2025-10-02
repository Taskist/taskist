using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.WorkItems;

namespace Taskist.Data.Configuration.WorkItems;

public class BacklogConfig : EntityTypeConfiguration<Backlog>
{
    public override void Configure(EntityTypeBuilder<Backlog> builder)
    {
        builder.ToTable(nameof(Backlog));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Code)
            .IsRequired();

        builder.Property(p => p.Title)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasOne(f => f.Parent)
            .WithMany()
            .HasForeignKey(p => p.ParentId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(f => f.TaskType)
            .WithMany()
            .HasForeignKey(p => p.TaskTypeId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        builder.HasOne(f => f.Severity)
           .WithMany()
           .HasForeignKey(p => p.SeverityId)
           .OnDelete(DeleteBehavior.NoAction)
           .IsRequired(true);

        builder.HasOne(f => f.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
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

        builder.HasOne(f => f.Sprint)
            .WithMany()
            .HasForeignKey(p => p.SprintId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(f => f.Reporter)
           .WithMany()
           .HasForeignKey(p => p.ReporterId)
           .OnDelete(DeleteBehavior.NoAction)
           .IsRequired(false);

        builder.HasOne(f => f.Assignee)
            .WithMany()
            .HasForeignKey(p => p.AssigneeId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(f => f.Status)
            .WithMany()
            .HasForeignKey(p => p.StatusId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        builder.HasOne(f => f.CreatedBy)
            .WithMany()
            .HasForeignKey(p => p.CreatedById)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        builder.HasOne(f => f.ModifiedBy)
            .WithMany()
            .HasForeignKey(p => p.ModifiedById)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        base.Configure(builder);
    }
}