using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Users;

namespace Taskist.Data.Configuration.Users;

public class UserRolePermissionConfig : EntityTypeConfiguration<UserRolePermission>
{
    public override void Configure(EntityTypeBuilder<UserRolePermission> builder)
    {
        builder.ToTable(nameof(UserRolePermission));

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.SystemName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.RoleGroup)
            .HasMaxLength(255)
            .IsRequired();

        base.Configure(builder);
    }
}