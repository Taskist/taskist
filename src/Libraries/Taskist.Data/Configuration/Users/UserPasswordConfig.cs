using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Users;

namespace Taskist.Data.Configuration.Users;

public class UserPasswordConfig : EntityTypeConfiguration<UserPassword>
{
    public override void Configure(EntityTypeBuilder<UserPassword> builder)
    {
        builder.ToTable(nameof(UserPassword));

        builder.HasKey(p => p.Id);

        builder.HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        base.Configure(builder);
    }
}