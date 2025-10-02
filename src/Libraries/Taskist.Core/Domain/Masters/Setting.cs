using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Masters;

public class Setting : BaseEntity
{
    public string Name { get; set; }

    public string Value { get; set; }
}