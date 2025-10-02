namespace Taskist.Web.Models.Masters
{
    public class CustomFieldValueModel
    {
        public int BacklogTaskId { get; set; }

        public int CustomFieldId { get; set; }

        public string Value { get; set; }
    }
}
