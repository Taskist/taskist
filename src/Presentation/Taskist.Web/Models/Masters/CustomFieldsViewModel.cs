namespace Taskist.Web.Models.Masters
{
    public class CustomFieldsViewModel
    {
        public CustomFieldsViewModel()
        {
            CustomFields = [];
            Values = [];
        }

        public int BacklogId { get; set; }

        public List<CustomFieldRenderModel> CustomFields { get; set; }

        public List<CustomFieldValueModel> Values { get; set; }
    }
}
