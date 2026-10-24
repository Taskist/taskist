namespace Taskist.Service.Security;

public interface IHtmlSanitizerService
{
    string Sanitize(string? html);
}
