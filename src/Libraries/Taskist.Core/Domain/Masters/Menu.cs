using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Masters;

	public class Menu : BaseEntity
{
		public string Name { get; set; }

		public string SystemName { get; set; }

		public int Code { get; set; }

		public string? ActionName { get; set; }

		public string? ControllerName { get; set; }

		public int? ParentCode { get; set; }

		public string? Icon { get; set; }

		public int DisplayOrder { get; set; }

		public string Permission { get; set; }

		public bool Active { get; set; }
	}