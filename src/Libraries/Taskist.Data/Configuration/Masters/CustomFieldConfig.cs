using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Masters;

namespace Taskist.Data.Configuration.Masters;

public class CustomFieldConfig : EntityTypeConfiguration<CustomField>
{
    public override void Configure(EntityTypeBuilder<CustomField> builder)
    {
        builder.ToTable(nameof(CustomField));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.ResourceKey)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.ColumnClass)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne(f => f.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        base.Configure(builder);
    }
}