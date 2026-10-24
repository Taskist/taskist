using Taskist.Core.Domain.Wiki;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.Wiki;

public class WikiCategoryConfig : EntityTypeConfiguration<WikiCategory>
{
    public override void Configure(EntityTypeBuilder<WikiCategory> builder)
    {
        builder.ToTable(nameof(WikiCategory));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(250);

        builder.HasOne(f => f.ParentCategory)
            .WithMany()
            .HasForeignKey(p => p.ParentCategoryId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        builder.HasOne(f => f.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(false);

        base.Configure(builder);
    }
}