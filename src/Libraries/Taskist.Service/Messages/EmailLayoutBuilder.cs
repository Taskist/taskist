using System.Text.RegularExpressions;

namespace Taskist.Service.Messages;

public static class EmailLayoutBuilder
{
    public static string Wrap(string innerHtml, string logoUrl, string company)
    {
        if (string.IsNullOrWhiteSpace(innerHtml))
            innerHtml = string.Empty;

        if (innerHtml.Contains("wp-email-shell", StringComparison.OrdinalIgnoreCase))
            return innerHtml;

        if (string.IsNullOrWhiteSpace(company))
            company = "Somaraj";

        var year = DateTime.Today.Year;
        var body = StyleLinks(innerHtml);

        return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""utf-8"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
</head>
<body class=""wp-email-shell"" style=""margin:0;padding:0;background:#eef2f7;"">
<table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#eef2f7;padding:24px 12px;font-family:'Segoe UI',Roboto,Arial,sans-serif;"">
  <tr><td align=""center"">
    <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px;width:100%;background:#ffffff;border-radius:12px;overflow:hidden;border:1px solid #e2e8f0;"">
      <tr><td align=""center"" style=""background:#ffffff;padding:24px 28px 18px;"">
        <img src=""{logoUrl}"" alt=""{company}"" height=""38"" style=""height:38px;display:block;border:0;outline:none;"" />
      </td></tr>
      <tr><td style=""font-size:0;line-height:0;height:3px;background:#1cc7c9;background-image:linear-gradient(90deg,#1cc7c9 0%,#ffc21a 100%);"">&nbsp;</td></tr>
      <tr><td style=""padding:30px 32px;color:#2b3a4b;font-size:15px;line-height:1.65;"">
        {body}
      </td></tr>
      <tr><td align=""center"" style=""padding:18px 28px;border-top:1px solid #eef2f7;color:#8a97a8;font-size:12px;"">
        &copy; {year} {company}
      </td></tr>
    </table>
  </td></tr>
</table>
</body>
</html>";
    }

    private static string StyleLinks(string html)
    {
        return Regex.Replace(html,
            "<a (?![^>]*style=)",
            "<a style=\"color:#0f9a9c;font-weight:600;text-decoration:none;\" ",
            RegexOptions.IgnoreCase);
    }
}
