using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Taskist.Data.Configuration.Users;

public class UserPasswordConfig : EntityTypeConfiguration<UserPassword>
{
    public override void Configure(EntityTypeBuilder<UserPassword> builder)
    {
        builder.ToTable(nameof(UserPassword));

        builder.HasKey(p => p.Id);

        //existing rows predate the format column and hold SHA1 hashes
        builder.Property(p => p.HashFormat)
            .HasDefaultValue((int)PasswordFormat.Sha1Legacy);

        builder.HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        base.Configure(builder);
    }
}