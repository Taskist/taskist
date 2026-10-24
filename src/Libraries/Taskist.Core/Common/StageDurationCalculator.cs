namespace Taskist.Core.Common;

public static class StageDurationCalculator
{
    #region First-pass span

    public static (DateTime From, DateTime To)? FirstPassSpan(
        IEnumerable<(int StatusId, DateTime On)> orderedLog, int fromStatusId, int toStatusId)
    {
        DateTime? from = null;
        foreach (var entry in orderedLog)
        {
            if (from == null)
            {
                if (entry.StatusId == fromStatusId)
                    from = entry.On;
                continue;
            }

            if (entry.StatusId == toStatusId && entry.On >= from.Value)
                return (from.Value, entry.On);
        }

        return null;
    }

    #endregion

    #region Smart d/h/m formatting

    public static string SmartText(double totalDays)
    {
        if (totalDays < 0)
            totalDays = 0;

        var span = TimeSpan.FromDays(totalDays);
        var days = (int)span.TotalDays;
        var hours = span.Hours;
        var minutes = span.Minutes;

        if (days >= 1)
            return hours > 0 ? $"{days}d {hours}h" : $"{days}d";

        if (hours >= 1)
            return minutes > 0 ? $"{hours}h {minutes}m" : $"{hours}h";

        return $"{Math.Max(0, minutes)}m";
    }

    #endregion
}
