namespace Taskist.Web.Helpers.Documents;

public interface IUploadTokenService
{
    string Create(int userId, int documentId);

    int? Read(string? token, int userId);
}
