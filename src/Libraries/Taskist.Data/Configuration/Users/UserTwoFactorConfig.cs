using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Users;

namespace Taskist.Data.Configuration.Users;

public class UserTwoFactorConfig : EntityTypeConfiguration<UserTwoFactor>
{
    public override void Configure(EntityTypeBuilder<UserTwoFactor> builder)
    {
        builder.ToTable(nameof(UserTwoFactor));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Secret)
            .IsRequired();

        builder.HasIndex(x => x.UserId)
            .IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        base.Configure(builder);
    }
}
