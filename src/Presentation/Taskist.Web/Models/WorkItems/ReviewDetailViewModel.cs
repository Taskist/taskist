using Taskist.Core.Dtos.Git;

namespace Taskist.Web.Models.WorkItems;

public class ReviewDetailViewModel
{
	public ReviewDetailDto Detail { get; set; } = new();

	public bool CanMergePermission { get; set; }
}
