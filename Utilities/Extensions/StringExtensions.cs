using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace AccountManager.Utilities.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// Checks if a string is null, empty, or whitespace
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        /// <summary>
        /// Truncates a string to a specified length
        /// </summary>
        public static string Truncate(this string value, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength - suffix.Length) + suffix;
        }

        /// <summary>
        /// Converts string to title case
        /// </summary>
        public static string ToTitleCase(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }

        /// <summary>
        /// Removes all whitespace from a string
        /// </summary>
        public static string RemoveWhitespace(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return Regex.Replace(value, @"\s+", "");
        }

        /// <summary>
        /// Masks sensitive information with asterisks
        /// </summary>
        public static string Mask(this string value, int visibleChars = 0, char maskChar = '*')
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (visibleChars >= value.Length)
                return value;

            var visible = value.Substring(0, visibleChars);
            var masked = new string(maskChar, Math.Max(0, value.Length - visibleChars));
            return visible + masked;
        }

        /// <summary>
        /// Converts a string to a safe filename
        /// </summary>
        public static string ToSafeFileName(this string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var invalidChars = System.IO.Path.GetInvalidFileNameChars();
            var result = value;
            
            foreach (var c in invalidChars)
            {
                result = result.Replace(c, '_');
            }
            
            return result;
        }

        /// <summary>
        /// Checks if string contains any of the specified values (case insensitive)
        /// </summary>
        public static bool ContainsAny(this string value, params string[] values)
        {
            if (string.IsNullOrEmpty(value) || values == null)
                return false;

            foreach (var item in values)
            {
                if (value.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            
            return false;
        }
    }
}