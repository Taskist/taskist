using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Taskist.Core.Domain.Users;

namespace Taskist.Data.Configuration.Users;

public class UserProjectMapConfig : EntityTypeConfiguration<UserProjectMap>
{
    public override void Configure(EntityTypeBuilder<UserProjectMap> builder)
    {
        builder.ToTable(nameof(UserProjectMap));

        builder.HasKey(x => x.Id);

        builder.HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        builder.HasOne(f => f.Project)
            .WithMany()
            .HasForeignKey(p => p.ProjectId)
            .OnDelete(DeleteBehavior.NoAction)
            .IsRequired(true);

        base.Configure(builder);
    }
}