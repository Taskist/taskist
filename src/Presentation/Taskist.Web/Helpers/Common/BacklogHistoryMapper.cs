using Taskist.Core.Common;
using Taskist.Core.Domain.WorkItems;
using Taskist.Web.Models.WorkItems;

namespace Taskist.Web.Helpers.Common;

public static class BacklogHistoryMapper
{
    #region Methods

    public static List<BacklogCommentGridModel> ToGridModels(IEnumerable<BacklogComment> entries)
    {
        return entries.Select(s =>
        {
            var item = new BacklogCommentGridModel
            {
                CommentById = s.CreatedById,
                CommentBy = s.CreatedBy?.Name,
                CommentOn = s.CreatedOn,
                Comment = s.Comment,
                SystemComment = s.SystemComment
            };
            ApplyTextChange(item, s.FieldName, s.OldValue, s.NewValue);
            return item;
        }).ToList();
    }

    #endregion

    #region Utilities

    private static void ApplyTextChange(BacklogCommentGridModel item, string? fieldName, string? oldValue, string? newValue)
    {
        if (string.IsNullOrEmpty(fieldName))
        {
            var colon = item.Comment?.IndexOf(": ", StringComparison.Ordinal) ?? -1;
            if (colon <= 0)
                return;

            fieldName = BacklogTextFields.FromLabel(item.Comment![..colon]);
            if (fieldName == null)
                return;

            var body = item.Comment[(colon + 2)..];
            var parts = body.Split(" → ");
            if (parts.Length != 2)
                return;

            oldValue = parts[0];
            newValue = parts[1];
        }

        item.FieldLabel = fieldName.Beautify();
        item.OldValue = oldValue;
        item.NewValue = newValue;
        item.ChangeAction = string.IsNullOrWhiteSpace(oldValue) ? "added"
            : string.IsNullOrWhiteSpace(newValue) ? "cleared"
            : "updated";
    }

    #endregion
}
