using Taskist.Core.Dtos.Git;

namespace Taskist.Web.Models.WorkItems;

public class MyCodeViewModel
{
    public MyCodeDto Code { get; set; } = new();

    public bool CanReview { get; set; }
}
