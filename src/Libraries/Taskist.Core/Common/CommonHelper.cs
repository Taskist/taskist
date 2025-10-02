using System.ComponentModel;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace Taskist.Core.Common;

public static class CommonHelper
{
    #region Fields

    private const string EmailRegexPattern =
        @"^((([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#\$%&'\*\+\-\/=\?\^_`{\|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-||_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+([a-z]+|\d|-|\.{0,1}|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])?([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))$";

    private static readonly Regex EmailRegex;

    #endregion

    #region Ctor

    static CommonHelper()
    {
        EmailRegex = new Regex(EmailRegexPattern, RegexOptions.IgnoreCase);
    }

    #endregion

    #region Methods

    public static bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        email = email.Trim();

        return EmailRegex.IsMatch(email);
    }

    public static bool IsValidIPAddress(string ipAddress)
    {
        return IPAddress.TryParse(ipAddress, out var _);
    }

    public static string GenerateRandomDigitCode(int length)
    {
        using var random = new RandomNumberHelper();
        var str = string.Empty;
        for (var i = 0; i < length; i++)
            str = string.Concat(str, random.Next(10).ToString());
        return str;
    }

    public static int GenerateRandomInteger(int min = 0, int max = int.MaxValue)
    {
        using var random = new RandomNumberHelper();
        return random.Next(min, max);
    }

    public static string EnsureMaximumLength(string str, int maxLength, string postfix = null)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        if (str.Length <= maxLength)
            return str;

        var pLen = postfix?.Length ?? 0;

        var result = str.Substring(0, maxLength - pLen);
        if (!string.IsNullOrEmpty(postfix))
        {
            result += postfix;
        }

        return result;
    }

    public static string EnsureNumericOnly(string str)
    {
        return string.IsNullOrEmpty(str) ? string.Empty : new string(str.Where(char.IsDigit).ToArray());
    }

    public static string EnsureNotNull(string str)
    {
        return str ?? string.Empty;
    }

    public static object To(object value, Type destinationType)
    {
        return To(value, destinationType, CultureInfo.InvariantCulture);
    }

    public static T To<T>(object value)
    {
        return (T)To(value, typeof(T));
    }

    public static object To(object value, Type destinationType, CultureInfo culture)
    {
        if (value == null)
            return null;

        var sourceType = value.GetType();

        var destinationConverter = TypeDescriptor.GetConverter(destinationType);
        if (destinationConverter.CanConvertFrom(value.GetType()))
            return destinationConverter.ConvertFrom(null, culture, value);

        var sourceConverter = TypeDescriptor.GetConverter(sourceType);
        if (sourceConverter.CanConvertTo(destinationType))
            return sourceConverter.ConvertTo(null, culture, value, destinationType);

        if (destinationType.IsEnum && value is int)
            return Enum.ToObject(destinationType, (int)value);

        if (!destinationType.IsInstanceOfType(value))
            return Convert.ChangeType(value, destinationType, culture);

        return value;
    }

    public static object? GetPropertyValue(object entity, string propertyPath)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        if (string.IsNullOrWhiteSpace(propertyPath))
            throw new ArgumentException("Property path cannot be null or whitespace.", nameof(propertyPath));

        var propertyNames = propertyPath.Split('.');

        object currentObject = entity;

        foreach (var propertyName in propertyNames)
        {
            if (currentObject == null)
                return null;

            var propertyInfo = currentObject.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
                throw new ArgumentException($"Property '{propertyName}' does not exist on type '{currentObject.GetType().Name}'.");

            currentObject = propertyInfo.GetValue(currentObject);
        }

        return currentObject;
    }

    public static string Beautify(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        string trimmed = Regex.Replace(input, "Id$", "", RegexOptions.IgnoreCase);

        string beautified = Regex.Replace(trimmed, "(?<!^)([A-Z])", " $1");

        return beautified.Trim();
    }

    #endregion
}