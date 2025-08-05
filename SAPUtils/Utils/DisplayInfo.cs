using SAPUtils.__Internal.Models;
using SAPUtils.Database;

namespace SAPUtils.Utils {
    /// <summary>
    /// Provides utility functionality for retrieving display-related information such as formatting configurations.
    /// </summary>
    /// <remarks>
    /// This class acts as a helper for fetching formatting information that can be used throughout the application.
    /// </remarks>
    /// <see cref="SAPUtils.__Internal.Models.FormatInfo"/>
    /// <see cref="SAPUtils.Database.Repository"/>
    public static class DisplayInfo {

        private static FormatInfo _formatInfo;

        /// <summary>
        /// Represents the format settings used within the system for various numeric
        /// and date/time formatting purposes. This includes details such as the number
        /// of decimal places for different numeric values, separators for decimals
        /// and thousands, and formats for dates and times.
        /// </summary>
        /// <remarks>
        /// The <see cref="FormatInfo"/> class encapsulates culture-sensitive formatting details,
        /// which can be retrieved from a repository. It is used to standardize the way
        /// numbers, dates, and times are formatted throughout the application.
        /// </remarks>
        /// <seealso cref="SAPUtils.Database.Repository"/>
        public static FormatInfo FormatInfo => _formatInfo ?? (_formatInfo = ((Repository)Repository.Get()).GetFormatInformation());
    }
}