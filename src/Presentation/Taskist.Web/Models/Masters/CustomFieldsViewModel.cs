namespace Taskist.Web.Models.Masters
{
    public class CustomFieldsViewModel
    {
        public CustomFieldsViewModel()
        {
            CustomFields = [];
        }

        public bool InsideRow { get; set; }

        public bool CanUpdate { get; set; }

        public List<CustomFieldRenderModel> CustomFields { get; set; }
    }
}
