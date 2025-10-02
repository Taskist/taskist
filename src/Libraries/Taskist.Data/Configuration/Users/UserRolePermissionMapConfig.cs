using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Users;

namespace Taskist.Data.Configuration.Users;

public class UserRolePermissionMapConfig : EntityTypeConfiguration<UserRolePermissionMap>
{
    public override void Configure(EntityTypeBuilder<UserRolePermissionMap> builder)
    {
        builder.ToTable(nameof(UserRolePermissionMap));

        builder.HasKey(p => new
        {
            p.PermissionId,
            p.UserRoleId
        });

        builder.HasOne(f => f.UserRole)
            .WithMany(p => p.UserRolePermissionMaps)
            .HasForeignKey(p => p.UserRoleId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(f => f.Permission)
            .WithMany(p => p.UserRolePermissionMaps)
            .HasForeignKey(p => p.PermissionId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Ignore(mapping => mapping.Id);

        base.Configure(builder);
    }
}