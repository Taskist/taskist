using Taskist.Core.Domain.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taskist.Data.Configuration.Logging;

public class LogConfig : EntityTypeConfiguration<Log>
{
    public override void Configure(EntityTypeBuilder<Log> builder)
    {
        builder.ToTable(nameof(Log));

        builder.HasKey(x => x.Id);

        base.Configure(builder);
    }
}