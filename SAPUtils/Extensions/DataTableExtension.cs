using System;
using System.Globalization;
using SAPbouiCOM;

namespace SAPUtils.Extensions {
    public static class DataTableExtension {
        public static string GetString(this DataTable dt, string column, int row) {
            string value = dt.GetValue(column, row)?.ToString();
            return string.IsNullOrWhiteSpace(value) ? "" : value.Trim();
        }
        public static int GetInt(this DataTable dt, string column, int row) {
            string val = dt.GetValue(column, row)?.ToString();
            return int.TryParse(val, out int result) ? result : 0;
        }
        public static long GetLong(this DataTable dt, string column, int row) {
            string val = dt.GetValue(column, row)?.ToString();
            return long.TryParse(val, out long result) ? result : 0;
        }
        public static double GetDouble(this DataTable dt, string column, int row) {
            string val = dt.GetValue(column, row)?.ToString();
            return double.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out double result) ? result : 0;
        }
        public static double GetFloat(this DataTable dt, string column, int row) {
            string val = dt.GetValue(column, row)?.ToString();
            return float.TryParse(val, NumberStyles.Any, CultureInfo.InvariantCulture, out float result) ? result : 0;
        }
        public static DateTime GetDate(this DataTable dt, string column, int row) {
            object value = dt.GetValue(column, row);
            if (value is DateTime d) return d;

            return DateTime.TryParseExact(value.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result)
                ? result
                : default;
        }
    }
}