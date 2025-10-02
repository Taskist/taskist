using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Masters;

namespace Taskist.Data.Configuration.Masters;

public class SettingConfig : EntityTypeConfiguration<Setting>
{
    public override void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable(nameof(Setting));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(250)
            .IsRequired();

        base.Configure(builder);
    }
}