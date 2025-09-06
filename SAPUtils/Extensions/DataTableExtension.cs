using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using SAPbouiCOM;

namespace SAPUtils.Extensions {
    /// <summary>
    /// Provides extension methods for <see cref="DataTable"/> to safely retrieve
    /// strongly-typed values (string, int, double, etc.) from SAP Business One data tables.
    /// </summary>
    [Localizable(false)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public static class DataTableExtension {
        /// <summary>
        /// Gets the string value from the specified column and row.
        /// Returns an empty string if the value is null or whitespace.
        /// </summary>
        /// <param name="dt">The SAP Business One <see cref="DataTable"/>.</param>
        /// <param name="column">The column name.</param>
        /// <param name="row">The row index.</param>
        /// <returns>The trimmed string value, or an empty string if null/whitespace.</returns>
        public static string GetString(this DataTable dt, string column, int row) {
            string value = dt.GetValue(column, row)?.ToString();
            return string.IsNullOrWhiteSpace(value) ? "" : value.Trim();
        }

        /// <summary>
        /// Gets the boolean value from the specified column and row.
        /// Returns true if the value is "Y" (case-sensitive), otherwise false.
        /// Null or empty values are treated as "N".
        /// </summary>
        /// <param name="dt">The SAP Business One <see cref="DataTable"/>.</param>
        /// <param name="column">The column name.</param>
        /// <param name="row">The row index.</param>
        /// <returns><c>true</c> if the value is "Y"; otherwise, <c>false</c>.</returns>
        public static bool GetBoolean(this DataTable dt, string column, int row) {
            string value = dt.GetValue(column, row)?.ToString();
            return (string.IsNullOrWhiteSpace(value) ? "N" : value.Trim()) == "Y";
        }

        /// <summary>
        /// Gets the integer value from the specified column and row.
        /// Returns 0 if the value cannot be parsed.
        /// </summary>
        /// <param name="dt">The SAP Business One <see cref="DataTable"/>.</param>
        /// <param name="column">The column name.</param>
        /// <param name="row">The row index.</param>
        /// <returns>The parsed integer value, or 0 if parsing fails.</returns>
        public static int GetInt(this DataTable dt, string column, int row) {
            string val = dt.GetValue(column, row)?.ToString();
            return int.TryParse(val, out int result) ? result : 0;
        }

        /// <summary>
        /// Gets the long value from the specified column and row.
        /// Returns 0 if the value cannot be parsed.
        /// </summary>
        /// <param name="dt">The SAP Business One <see cref="DataTable"/>.</param>
        /// <param name="column">The column name.</param>
        /// <param name="row">The row index.</param>
        /// <returns>The parsed long value, or 0 if parsing fails.</returns>
        public static long GetLong(this DataTable dt, string column, int row) {
            string val = dt.GetValue(column, row)?.ToString();
            return long.TryParse(val, out long result) ? result : 0;
        }

        /// <summary>
        /// Gets the double value from the specified column and row.
        /// Returns 0 if the value cannot be parsed.
        /// Parsing uses <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        /// <param name="dt">The SAP Business One <see cref="DataTable"/>.</param>
        /// <param name="column">The column name.</param>
        /// <param name="row">The row index.</param>
        /// <returns>The parsed double value, or 0 if parsing fails.</returns>
        public static double GetDouble(this DataTable dt, string column, int row) {
            string val = dt.GetValue(column, row)?.ToString();
            return double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out double result) ? result : 0;
        }

        /// <summary>
        /// Gets the float value from the specified column and row.
        /// Returns 0 if the value cannot be parsed.
        /// Parsing uses <see cref="CultureInfo.InvariantCulture"/>.
        /// </summary>
        /// <param name="dt">The SAP Business One <see cref="DataTable"/>.</param>
        /// <param name="column">The column name.</param>
        /// <param name="row">The row index.</param>
        /// <returns>The parsed float value, or 0 if parsing fails.</returns>
        public static double GetFloat(this DataTable dt, string column, int row) {
            string val = dt.GetValue(column, row)?.ToString();
            return float.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out float result) ? result : 0;
        }

        /// <summary>
        /// Gets the date value from the specified column and row.
        /// If the value is already a <see cref="DateTime"/>, it is returned directly.
        /// Otherwise, attempts to parse it as a string in "yyyyMMdd" format.
        /// Returns the default value of <see cref="DateTime"/> if parsing fails.
        /// </summary>
        /// <param name="dt">The SAP Business One <see cref="DataTable"/>.</param>
        /// <param name="column">The column name.</param>
        /// <param name="row">The row index.</param>
        /// <returns>The parsed <see cref="DateTime"/> value, or default if parsing fails.</returns>
        public static DateTime GetDate(this DataTable dt, string column, int row) {
            object value = dt.GetValue(column, row);
            if (value is DateTime d) return d;

            return DateTime.TryParseExact(value.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result)
                ? result
                : default;
        }
    }
}