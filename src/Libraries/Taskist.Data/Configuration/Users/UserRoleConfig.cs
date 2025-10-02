using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Users;

namespace Taskist.Data.Configuration.Users;

public class UserRoleConfig : EntityTypeConfiguration<UserRole>
{
    public override void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable(nameof(UserRole));

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(250);

        builder.Property(p => p.SystemName)
            .HasMaxLength(50);

        builder.Property(p => p.SystemDefined)
            .IsRequired();

        base.Configure(builder);
    }
}