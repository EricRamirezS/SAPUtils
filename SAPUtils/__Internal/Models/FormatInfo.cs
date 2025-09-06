using System;
using System.Diagnostics.CodeAnalysis;
using SAPUtils.I18N;

namespace SAPUtils.__Internal.Models {
    /// <summary>
    /// Represents formatting information used for various operations such as
    /// handling numbers, dates, and other display formats.
    /// </summary>
    /// <remarks>
    /// This class provides properties that define decimal precision, separators,
    /// and date-time formatting conventions.
    /// </remarks>
    /// <see cref="SAPUtils.Utils.DisplayInfo"/>
    /// <see cref="SAPUtils.Database.Repository"/>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class FormatInfo {
        /// <summary>
        /// Represents the number of decimal places used for sum values.
        /// This property typically helps format or display numeric data consistently within the system.
        /// </summary>
        /// <seealso cref="SAPUtils.__Internal.Models.FormatInfo"/>
        public int SumDec { get; set; }

        /// <summary>
        /// Represents the number of decimal places to be used for price values.
        /// </summary>
        /// <remarks>
        /// This property is typically used to define the level of precision required for price-related operations
        /// within the application.
        /// </remarks>
        /// <seealso cref="SAPUtils.__Internal.Models.FormatInfo"/>
        public int PriceDec { get; set; }

        /// <summary>
        /// Represents the number of decimal places used for rate values within
        /// the <see cref="SAPUtils.__Internal.Models.FormatInfo"/> class.
        /// </summary>
        /// <remarks>
        /// This property determines the precision of rates, affecting how rate-related
        /// information is formatted and processed.
        /// </remarks>
        /// <seealso cref="SAPUtils.__Internal.Models.FormatInfo"/>
        public int RateDec { get; set; }

        /// <summary>
        /// Represents the number of decimal places configured for quantities in a specific format setting.
        /// This property is used to determine how quantity values are formatted and displayed.
        /// </summary>
        /// <remarks>
        /// This property is typically loaded as part of the <c cref="SAPUtils.__Internal.Models.FormatInfo"/> object.
        /// </remarks>
        /// <seealso cref="SAPUtils.__Internal.Models.FormatInfo"/>
        public int QtyDec { get; set; }

        /// <summary>
        /// Represents the number of decimal places used for percentage values in formatting settings.
        /// </summary>
        /// <remarks>
        /// This property is part of the <see cref="SAPUtils.__Internal.Models.FormatInfo"/> class and provides
        /// configuration for decimal precision in percentage-based values.
        /// </remarks>
        public int PercentDec { get; set; }

        /// <summary>
        /// Represents the number of decimal places configured for measurements.
        /// </summary>
        /// <remarks>
        /// This property is typically used to format measurement data according to system or user-defined precision settings.
        /// </remarks>
        /// <seealso cref="SAPUtils.__Internal.Models.FormatInfo"/>
        public int MeasureDec { get; set; }

        /// <summary>
        /// Represents the number of decimal places to be used in query-related formatting operations.
        /// </summary>
        /// <remarks>
        /// This property is typically used to specify the precision required for numerical values in query outputs.
        /// </remarks>
        /// <seealso cref="SAPUtils.Database.Repository"/>
        public int QueryDec { get; set; }

        /// <summary>
        /// Gets or sets the character used as the decimal separator in numeric values.
        /// </summary>
        /// <remarks>
        /// This property defines the character used to represent the decimal point
        /// in numeric representations within the system. Common values can include "." (dot)
        /// or "," (comma), depending on the regional settings or formatting requirements.
        /// </remarks>
        /// <seealso cref="SAPUtils.__Internal.Models.FormatInfo"/>
        public string DecSep { get; set; }

        /// <summary>
        /// Gets or sets the character used as a thousands separator in numerical representations.
        /// </summary>
        /// <remarks>
        /// The thousands separator is used to improve the readability of large numbers
        /// by grouping digits into sets, typically separated by a specific character (e.g., a comma or dot).
        /// </remarks>
        /// <seealso cref="SAPUtils.__Internal.Models.FormatInfo"/>
        public string ThousSep { get; set; }

        /// <summary>
        /// Represents the format used for displaying time values.
        /// </summary>
        /// <remarks>
        /// The property holds an integer that maps to different time formats based
        /// on system or application-specific configurations.
        /// </remarks>
        /// <seealso cref="FormatInfo"/>
        public int TimeFormat { get; set; }

        /// <summary>
        /// Represents the numeric value that defines the date format used in the application.
        /// The specific format is determined using predefined options, where each value corresponds
        /// to a specific date presentation, such as "dd/MM/yy", "yyyy-MM-dd", etc.
        /// </summary>
        /// <remarks>
        /// The value of <c>DateFormat</c> determines the layout of dates throughout the application.
        /// It works in conjunction with the <c>DateSep</c> property to specify the date separator.
        /// Refer to the <c>GetDateFullFormat</c> method for a detailed mapping of values to date formats.
        /// </remarks>
        /// <seealso cref="SAPUtils.__Internal.Models.FormatInfo.GetDateFullFormat"/>
        public int DateFormat { get; set; }

        /// <summary>
        /// Gets or sets the string used as the date separator in date formatting.
        /// </summary>
        /// <remarks>
        /// The <c>DateSep</c> property is utilized in conjunction with the <c>DateFormat</c> property to determine
        /// the formatting of dates when converting to or from string representations.
        /// </remarks>
        /// <seealso cref="FormatInfo.DateFormat"/>
        public string DateSep { get; set; }

        /// <summary>
        /// Retrieves the full date format string based on the specified DateFormat property and DateSep separator.
        /// </summary>
        /// <returns>
        /// A string representing the full date format, customized according to the DateFormat property and
        /// the selected date separator.
        /// </returns>
        /// <exception cref="System.NotImplementedException">
        /// Thrown when the specified DateFormat property does not match any of the handled cases.
        /// </exception>
        public string GetDateFullFormat() {
            switch (DateFormat) {
                // ReSharper disable LocalizableElement
                case 0:
                    return $"dd{DateSep}MM{DateSep}yy";
                case 1:
                    return $"dd{DateSep}MM{DateSep}yyyy";
                case 2:
                    return $"MM{DateSep}dd{DateSep}yy";
                case 3:
                    return $"MM{DateSep}dd{DateSep}yyyy";
                case 4:
                    return $"yyyy{DateSep}MM{DateSep}dd";
                case 5:
                    return $"dd{DateSep}MMM{DateSep}yyyy";
                case 6:
                    return $"yy{DateSep}MM{DateSep}dd";
                // ReSharper restore LocalizableElement
                default:
                    throw new ArgumentOutOfRangeException(nameof(DateFormat), DateFormat, Texts.FormatInfo_GetDateFullFormat_Unrecognized_date_format_);
            }
        }
    }
}