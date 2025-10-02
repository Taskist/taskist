using Microsoft.AspNetCore.Mvc.Rendering;
using Taskist.Web.Helpers.Attributes;
using Taskist.Web.Models.Common;

namespace Taskist.Web.Models.Masters
{
    public class CustomFieldModel : BaseModel
    {
        public CustomFieldModel()
        {
            AvailableColumnClasses = [];
            for (var i = 1; i <= 12; i++)
            {
                AvailableColumnClasses.Add(new SelectListItem
                {
                    Value = i.ToString(),
                    Text = $"{i} column"
                });
            }
        }

        [LocalizedDisplayName("CustomFieldModel.Label")]
        public string Label { get; set; }

        [LocalizedDisplayName("CustomFieldModel.ResourceKey")]
        public string ResourceKey { get; set; }

        [LocalizedDisplayName("CustomFieldModel.FieldType")]
        public int FieldType { get; set; }

        [LocalizedDisplayName("CustomFieldModel.Options")]
        public string? Options { get; set; }

        [LocalizedDisplayName("CustomFieldModel.Mandatory")]
        public bool Mandatory { get; set; }

        [LocalizedDisplayName("CustomFieldModel.ColumnClass")]
        public int ColumnClass { get; set; }

        [LocalizedDisplayName("CustomFieldModel.Placement")]
        public int Placement { get; set; }

        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public IList<SelectListItem> AvailableColumnClasses { get; set; }
    }

    public class CustomFieldGridModel : BaseModel
    {
        public string Label { get; set; }

        public string ResourceKey { get; set; }

        public string FieldType { get; set; }

        public bool Mandatory { get; set; }

        public string ColumnClass { get; set; }

        public string Placement { get; set; }
    }
}
