using Taskist.Core.Domain.Git;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.Git;

public class UserGitAccountConfig : EntityTypeConfiguration<UserGitAccount>
{
    public override void Configure(EntityTypeBuilder<UserGitAccount> builder)
    {
        builder.ToTable(nameof(UserGitAccount));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.ExternalUserId)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(p => p.UserName)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.DisplayName)
            .HasMaxLength(255);

        builder.Property(p => p.AccessToken)
            .IsRequired();

        builder.Property(p => p.Scopes)
            .HasMaxLength(500);

        builder.HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasOne(f => f.GitConnection)
            .WithMany()
            .HasForeignKey(p => p.GitConnectionId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired();

        builder.HasIndex(p => new { p.UserId, p.GitConnectionId })
            .IsUnique();

        builder.HasIndex(p => new { p.GitConnectionId, p.ExternalUserId })
            .IsUnique();

        base.Configure(builder);
    }
}
