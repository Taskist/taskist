using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Users;

namespace Taskist.Data.Configuration.Users;

public class UserConfig : EntityTypeConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(nameof(User));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Code)
            .IsRequired();

        builder.Property(p => p.FirstName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.LastName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Email)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(p => p.LastIPAddress)
            .HasMaxLength(100);

        builder.HasOne(f => f.Language)
            .WithMany()
            .HasForeignKey(p => p.LanguageId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        builder.Ignore(p => p.UserRoles);
        builder.Ignore(p => p.IsAdmin);

        base.Configure(builder);
    }
}