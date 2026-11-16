using Taskist.Core.Domain.Git;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.Git;

public class GitConnectionConfig : EntityTypeConfiguration<GitConnection>
{
    public override void Configure(EntityTypeBuilder<GitConnection> builder)
    {
        builder.ToTable(nameof(GitConnection));

        builder.HasKey(x => x.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.BaseUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.ApiUrl)
            .HasMaxLength(500);

        builder.Property(p => p.AppId)
            .HasMaxLength(50);

        builder.Property(p => p.OAuthClientId)
            .HasMaxLength(200);

        builder.Property(p => p.ServiceAccountUserName)
            .HasMaxLength(255);

        builder.Property(p => p.ServiceAccountExternalId)
            .HasMaxLength(64);

        base.Configure(builder);
    }
}
