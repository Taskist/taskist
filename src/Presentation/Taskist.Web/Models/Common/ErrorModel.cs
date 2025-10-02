namespace Taskist.Web.Models.Common;

public class ErrorModel
{
    public ErrorModel(string key, string? message)
    {
        Key = key;
        Message = message;
    }

    public string Key { get; set; }
    public string Message { get; set; }
}
