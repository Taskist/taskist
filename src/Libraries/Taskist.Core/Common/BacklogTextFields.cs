namespace Taskist.Core.Common;

public static class BacklogTextFields
{
    #region Fields

    public static readonly IReadOnlyList<string> Names =
    [
        "Description",
        "RootCauseAnalysis",
        "ResolutionDetails"
    ];

    #endregion

    #region Methods

    public static bool IsTextField(string? propertyName)
    {
        return !string.IsNullOrEmpty(propertyName) && Names.Contains(propertyName);
    }

    public static string? FromLabel(string? label)
    {
        if (string.IsNullOrWhiteSpace(label))
            return null;

        return Names.FirstOrDefault(n => string.Equals(n.Beautify(), label.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    #endregion
}
