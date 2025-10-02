using Microsoft.EntityFrameworkCore;

namespace Taskist.Data.Configuration;

public partial interface IMappingConfiguration
{
    void ApplyConfiguration(ModelBuilder modelBuilder);
}