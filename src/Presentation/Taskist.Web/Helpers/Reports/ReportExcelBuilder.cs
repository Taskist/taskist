using ClosedXML.Excel;

namespace Taskist.Web.Helpers.Reports;

public static class ReportExcelBuilder
{
    public static byte[] Build<T>(IEnumerable<T> rows, IList<ReportColumn> columns, string sheetName, string[] groupByColumns = null)
    {
        return Build(rows, columns, sheetName, null, groupByColumns);
    }

    public static byte[] Build<T>(IEnumerable<T> rows, IList<ReportColumn> columns, string sheetName, string[] titleLines, string[] groupByColumns)
    {
        groupByColumns ??= [];
        titleLines ??= [];

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(SafeSheetName(sheetName));

        var row = 1;

        if (titleLines.Length > 0 && columns.Count > 0)
        {
            foreach (var line in titleLines)
            {
                worksheet.Cell(row, 1).Value = line;
                worksheet.Cell(row, 1).Style.Font.Bold = true;
                if (columns.Count > 1)
                    worksheet.Range(row, 1, row, columns.Count).Merge();
                row++;
            }
            row++;
        }

        for (var i = 0; i < columns.Count; i++)
        {
            worksheet.Cell(row, i + 1).Value = columns[i].Title;
            worksheet.Cell(row, i + 1).Style.Font.Bold = true;
        }

        row++;

        if (groupByColumns.Length > 0)
        {
            var lastGroup = "";
            foreach (var item in rows)
            {
                var currentGroup = "";
                foreach (var groupCol in groupByColumns)
                {
                    var value = item.GetType().GetProperty(groupCol)?.GetValue(item, null);
                    currentGroup += (value == null ? "Unassigned" : value.ToString()) + " - ";
                }
                currentGroup = currentGroup.TrimEnd('-', ' ');

                if (currentGroup != lastGroup)
                {
                    worksheet.Cell(row, 1).Value = currentGroup;
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    worksheet.Row(row).OutlineLevel = 1;
                    row++;
                    lastGroup = currentGroup;
                }

                WriteRow(worksheet, row, columns, item);
                worksheet.Row(row).OutlineLevel = 2;
                row++;
            }
        }
        else
        {
            foreach (var item in rows)
            {
                WriteRow(worksheet, row, columns, item);
                row++;
            }
        }

        if (worksheet.LastCellUsed() != null)
            worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    #region Utilities

    private static void WriteRow<T>(IXLWorksheet worksheet, int row, IList<ReportColumn> columns, T item)
    {
        for (var i = 0; i < columns.Count; i++)
        {
            var value = item.GetType().GetProperty(columns[i].Key)?.GetValue(item, null);
            worksheet.Cell(row, i + 1).Value = ToCellValue(value);
        }
    }

    private static XLCellValue ToCellValue(object value)
    {
        return value switch
        {
            null => "",
            string s => s,
            bool b => b,
            DateTime dt => dt,
            DateTimeOffset dto => dto.DateTime,
            DateOnly d => d.ToDateTime(TimeOnly.MinValue),
            TimeSpan ts => ts,
            Enum e => e.ToString(),
            byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal => Convert.ToDouble(value),
            _ => value.ToString()
        };
    }

    private static string SafeSheetName(string name)
    {
        var invalid = new[] { ':', '\\', '/', '?', '*', '[', ']' };
        var cleaned = new string((name ?? "Sheet1").Select(c => invalid.Contains(c) ? '_' : c).ToArray()).Trim('\'');
        if (string.IsNullOrWhiteSpace(cleaned))
            cleaned = "Sheet1";
        return cleaned.Length > 31 ? cleaned[..31] : cleaned;
    }

    #endregion
}
