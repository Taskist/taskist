using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Users;

namespace Taskist.Data.Configuration.Users;

public class UserRecoveryCodeConfig : EntityTypeConfiguration<UserRecoveryCode>
{
    public override void Configure(EntityTypeBuilder<UserRecoveryCode> builder)
    {
        builder.ToTable(nameof(UserRecoveryCode));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CodeHash)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(x => x.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        base.Configure(builder);
    }
}
