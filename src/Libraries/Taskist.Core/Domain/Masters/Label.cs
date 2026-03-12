using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Taskist.Core.Domain.Common;

namespace Taskist.Core.Domain.Masters
{
    public class Label : BaseEntity, ISoftDeletedEntity
    {
        public Label() { }

        public string Name { get; set; }
        public string? Color { get; set; }
        public string? Description { get; set; }
               
        public bool Active { get; set; }
        public bool Deleted { get; set; }
    }
}