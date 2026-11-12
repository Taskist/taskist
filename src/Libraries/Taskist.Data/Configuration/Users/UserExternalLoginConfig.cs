using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Users;

namespace Taskist.Data.Configuration.Users;

public class UserExternalLoginConfig : EntityTypeConfiguration<UserExternalLogin>
{
    public override void Configure(EntityTypeBuilder<UserExternalLogin> builder)
    {
        builder.ToTable(nameof(UserExternalLogin));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Subject)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(150);

        builder.HasIndex(x => new { x.Provider, x.Subject })
            .IsUnique();

        builder.HasIndex(x => x.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        base.Configure(builder);
    }
}
