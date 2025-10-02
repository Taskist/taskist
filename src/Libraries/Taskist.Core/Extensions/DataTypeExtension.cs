using System.Globalization;
using System.Text.RegularExpressions;

namespace Taskist.Core.Extensions;

public static class DataTypeExtension
{
    #region Guid

    public static Guid ToGuid(this object value)
    {
        Guid.TryParse(value.ToString(), out var outValue);
        return outValue;
    }

    #endregion

    #region Currency

    public static string ToCurrency(this object value)
    {
        var globalization = false;
        decimal.TryParse(value.ToString(), out var outValue);
        if (globalization)
        {
            return outValue > 0 ? outValue.ToString("C") : "0";
        }

        return outValue > 0 ? outValue.ToString("N2") : "0";
    }

    #endregion

    #region Datetime
    public static bool IsDate(this string input)
    {
        if (!string.IsNullOrEmpty(input))
        {
            DateTime dt;
            return DateTime.TryParse(input, out dt);
        }
        else
        {
            return false;
        }
    }
    public static DateTime ToDateTime(this object value)
    {
        DateTime.TryParse(value.ToString(), out var outValue);
        return outValue;
    }

    public static DateOnly ToDateOnly(this object value)
    {
        DateOnly.TryParse(value.ToString(), out var outValue);
        return outValue;
    }

    public static string ToDateString(this object value, string format = "dd-MM-yyyy")
    {
        if (string.IsNullOrEmpty(value.ToString()))
            return null;
        DateTime.TryParse(value.ToString(), out var outValue);
        return outValue.ToString(format, CultureInfo.InvariantCulture);
    }

    public static string ToDateTimeString(this object value, string format = "dd-MM-yyyy hh:mm tt")
    {
        if (value == null || string.IsNullOrEmpty(value.ToString()))
            return null;
        DateTime.TryParse(value.ToString(), out var outValue);
        return outValue.ToString(format, CultureInfo.InvariantCulture);
    }

    public static string ToFileSizeString(this object value)
    {
        var byteCount = Convert.ToInt64(value);
        string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
        if (byteCount == 0)
            return "0" + suf[0];
        long bytes = Math.Abs(byteCount);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return (Math.Sign(byteCount) * num).ToString() + suf[place];
    }

    #endregion

    #region Integer

    public static int ToInt(this object value)
    {
        int.TryParse(value.ToString(), out var outValue);
        return outValue;
    }

    public static float ToFloat(this object value)
    {
        float.TryParse(value.ToString(), out var outValue);
        return outValue;
    }

    public static double ToDouble(this object value)
    {
        double.TryParse(value.ToString(), out var outValue);
        return outValue;
    }

    public static long ToLong(this object value)
    {
        long.TryParse(value.ToString(), out var outValue);
        return outValue;
    }

    #endregion

    #region Decimal

    public static decimal ToDecimal(this object value)
    {
        decimal.TryParse(value.ToString(), out var outValue);
        return outValue;
    }

    #endregion

    #region Bool

    public static bool ToBool(this object value)
    {
        try
        {
            return Convert.ToBoolean(value);
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Byte

    public static byte ToByte(this object value)
    {
        byte.TryParse(value.ToString(), out var outValue);
        return outValue;
    }

    #endregion

    #region String

    public static string ToDpInitial(this object value)
    {
        var initial = new string(value.ToString().Split(' ').Select(x => x[0]).ToArray());
        return initial;
    }

    public static string ToTitleCase(this object value)
    {
        if (string.IsNullOrEmpty(value.ToString()))
            return null;

        CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
        TextInfo textInfo = cultureInfo.TextInfo;
        return textInfo.ToTitleCase(value.ToString().ToLower());
    }

    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var formatted = Regex.Replace(input, @"([a-z])([A-Z])", "$1_$2");
        return formatted.ToLower();
    }

    #endregion

    #region Enum

    public static int ToInt<TValue>(this TValue value) where TValue : Enum
    {
        if (!typeof(int).IsAssignableFrom(Enum.GetUnderlyingType(typeof(TValue))))
            throw new ArgumentException(nameof(TValue));

        return (int)(object)value;
    }

    public static string ToString<TValue>(this int value) where TValue : Enum
    {
        if (Enum.IsDefined(typeof(TValue), value))
        {
            return ((TValue)(object)value).ToString();
        }
        else
        {
            return "Invalid enum value";
        }
    }

    #endregion
}