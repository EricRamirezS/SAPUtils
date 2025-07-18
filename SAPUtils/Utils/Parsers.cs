using System;
using System.Globalization;
using SAPUtils.Extensions;

namespace SAPUtils.Utils {
    /// <summary>
    /// Provides utility methods to facilitate parsing of various data types into .NET objects.
    /// </summary>
    /// <remarks>
    /// This class includes methods tailored to converting input values into standardized .NET types,
    /// such as <see cref="DateTime"/> or <see cref="double"/>, ensuring robust handling of valid and invalid inputs.
    /// </remarks>
    public static class Parsers {
        /// <summary>
        /// Parses an object into a <see cref="DateTime"/>. If the object is already a <see cref="DateTime"/>,
        /// it is returned directly. Otherwise, it tries to parse the object into a date using specific formats or defaults.
        /// </summary>
        /// <param name="value">The object value to be parsed into a <see cref="DateTime"/>.
        /// Supported formats include "yyyyMMdd" or other formats parsable by TryParse.</param>
        /// <returns>A parsed <see cref="DateTime"/> object if the input is successfully parsed; otherwise, <see cref="DateTime.MinValue"/>.</returns>
        /// <remarks>
        /// Useful for converting diverse object types into a standardized <see cref="DateTime"/>. Handles both exact
        /// format parsing and general parsing.
        /// </remarks>
        /// <seealso cref="DateTime"/>
        public static DateTime ParseDate(object value) {
            if (value is DateTime dt)
                return dt;

            if (DateTime.TryParseExact(
                    value?.ToString(),
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime result) || DateTime.TryParse(value?.ToString(), out result))
                return result;

            return DateTime.MinValue;
        }

        /// <summary>
        /// Parses the provided value into a <see cref="DateTime"/> object representing a time value.
        /// Returns <see cref="DateTime.MinValue"/> if parsing fails or the value is invalid.
        /// </summary>
        /// <param name="value">The object to be parsed. Accepted types include <see cref="DateTime"/>,
        /// an integer in the format HHmm (e.g., 1230 for 12:30 PM), or a string representation of time.</param>
        /// <returns>A <see cref="DateTime"/> object representing the parsed time value, or
        /// <see cref="DateTime.MinValue"/> if parsing fails or the value is invalid.</returns>
        /// <remarks>
        /// Attempts to interpret various input formats, such as integers in HHmm format, and strings with valid time formats.
        /// Ensures consistent date-time handling by returning a <see cref="DateTime"/> instance
        /// or <see cref="DateTime.MinValue"/> if conversion is unsuccessful.
        /// </remarks>
        /// <seealso cref="DateTime"/>
        /// <seealso cref="System.Globalization.DateTimeStyles"/>
        public static DateTime ParseTime(object value) {
            switch (value) {
                case null:
                    return DateTime.MinValue;
                case DateTime dt:
                    return dt;
                case int intValue when intValue < 0 || intValue > 2359:
                    return DateTime.MinValue;
                case int intValue:
                {
                    int minute = intValue % 100;

                    if (minute > 59)
                        return DateTime.MinValue;

                    int hour = intValue / 100;

                    return new DateTime(1, 1, 1, hour, minute, 0);
                }
                default:
                    if (DateTime.TryParseExact(
                            value.ToString().Trim().Replace(":", "", 1).PadLeft(4, '0'),
                            "HHmm",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out DateTime result) || DateTime.TryParse(value.ToString(), out result))
                        return result;
                    return DateTime.MinValue;
            }
        }
        /// <summary>
        /// Parses an object into a <see cref="double"/>. If the object is a numeric type, it is converted directly;
        /// otherwise, attempts to parse the object as a double from its string representation.
        /// </summary>
        /// <param name="value">The object to be parsed into a <see cref="double"/>. Supported types include numeric types
        /// (e.g., <see cref="int"/>, <see cref="decimal"/>, <see cref="float"/>, etc.) and strings parsable by
        /// TryParse using <see cref="CultureInfo.InvariantCulture"/>.</param>
        /// <returns>The parsed <see cref="double"/> value if the input can be converted or parsed successfully;
        /// otherwise, returns 0.</returns>
        /// <remarks>
        /// This method standardizes the parsing of various input types into a <see cref="double"/>,
        /// ensuring graceful handling of invalid formats by returning 0.
        /// </remarks>
        /// <seealso cref="double"/>
        /// <seealso cref="CultureInfo"/>
        public static object ParseDouble(object value) {
            switch (value) {
                case null:
                    return 0;
                case double d:
                    return d;
                case float f:
                    return (double)f;
                case decimal dec:
                    return (double)dec;
                case int i:
                    return (double)i;
                case long l:
                    return (double)l;
                default:
                {
                    string str = value.ToString();
                    return double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double result)
                        ? result
                        : 0;
                }
            }
        }
    }
}