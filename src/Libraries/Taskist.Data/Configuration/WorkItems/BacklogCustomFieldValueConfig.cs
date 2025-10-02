using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.WorkItems;

namespace Taskist.Data.Configuration.WorkItems;

public class BacklogCustomFieldValueConfig : EntityTypeConfiguration<BacklogCustomFieldValue>
{
    public override void Configure(EntityTypeBuilder<BacklogCustomFieldValue> builder)
    {
        builder.ToTable(nameof(BacklogCustomFieldValue));

        builder.HasKey(x => x.Id);

        builder.HasOne(f => f.Backlog)
            .WithMany()
            .HasForeignKey(p => p.BacklogId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        builder.HasOne(f => f.CustomField)
            .WithMany()
            .HasForeignKey(p => p.CustomFieldId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        base.Configure(builder);
    }
}