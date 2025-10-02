using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Users;

namespace Taskist.Data.Configuration.Users;

public class UserRoleMapConfig : EntityTypeConfiguration<UserRoleMap>
{
    public override void Configure(EntityTypeBuilder<UserRoleMap> builder)
    {
        builder.ToTable(nameof(UserRoleMap));

        builder.HasKey(p => new
        {
            p.UserId,
            p.UserRoleId
        });

        builder.HasOne(f => f.User)
            .WithMany(p => p.UserRoleMaps)
            .HasForeignKey(p => p.UserId)
            .IsRequired();

        builder.HasOne(p => p.UserRole)
            .WithMany()
            .HasForeignKey(p => p.UserRoleId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Ignore(p => p.Id);

        base.Configure(builder);
    }
}