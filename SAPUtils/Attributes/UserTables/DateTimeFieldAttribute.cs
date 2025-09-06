using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.I18N;
using SAPUtils.Utils;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to mark a class property as a SAP Business One user-defined field (UDF) 
    /// for storing date and time values.
    /// <br/>
    /// This attribute is designed for fields that store date and time data in SAP Business One, ensuring 
    /// the correct field type (`db_Date`) and default subtype (`st_None`).
    /// <br/>
    /// <b>Usage:</b><br/>
    /// - Apply this attribute to properties in a user table object model to define a date/time field.<br/>
    /// - Implements <see cref="IUserTableField{T}"/> for type-safe parsing and default value handling.<br/>
    /// <br/>
    /// <b>Example:</b>
    /// <code>
    /// [DateTimeUserTableField(Name = "OrderDate", Description = "The date when the order was placed", Mandatory = true)]
    /// public DateTime? OrderDate { get; set; }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class DateTimeFieldAttribute : UserTableFieldAttributeBase, IUserTableField<DateTime?> {

        private string _dateDescription;
        private DateTime? _stronglyTypedDefaultValue;
        private string _timeDescription;

        /// <summary>
        /// Gets or sets the description used for the date portion of this field in SAP Business One.
        /// <br/>
        /// <b>Important:</b> Unlike other field types, <see cref="DateTimeFieldAttribute"/> maps to 
        /// <b>two separate columns</b> in the database — one for the date and one for the time.
        /// This property allows customizing the human-readable label of the <b>date column</b>.
        /// <br/>
        /// If not explicitly set, it falls back to <see cref="UserTableFieldAttributeBase.Description"/> or defaults to <c>{Name}Date</c>.
        /// </summary>
        [Localizable(false)]
        public string DateDescription
        {
            get => _dateDescription ?? (Description ?? $"{Name}Date");
            set => _dateDescription = value;
        }

        /// <summary>
        /// Gets or sets the description used for the time portion of this field in SAP Business One.
        /// <br/>
        /// <b>Important:</b> Unlike other field types, <see cref="DateTimeFieldAttribute"/> maps to 
        /// <b>two separate columns</b> in the database — one for the date and one for the time.
        /// This property allows customizing the human-readable label of the <b>time column</b>.
        /// <br/>
        /// If not explicitly set, it falls back to <see cref="UserTableFieldAttributeBase.Description"/> or defaults to <c>{Name}Time</c>.
        /// </summary>
        [Localizable(false)]
        public string TimeDescription
        {
            get => _timeDescription ?? (Description ?? $"{Name}Time");
            set => _timeDescription = value;
        }

        /// <inheritdoc />
        public override int Size { get; set; } = 8;

        /// <inheritdoc />
        public override BoFieldTypes FieldType => BoFieldTypes.db_Date;

        /// <inheritdoc />
        public override object DefaultValue
        {
            get => _stronglyTypedDefaultValue;
            set => _stronglyTypedDefaultValue = (DateTime?)value;
        }

        /// <summary>
        /// ⚠️ This method is not intended to be used on its own for <see cref="DateTime"/> fields in SAP.
        /// Instead, use specialized methods designed for SAP's handling of date and time values separately.
        /// </summary>
        /// <param name="value">The value to be parsed.</param>
        /// <exception cref="NotSupportedException">
        /// Thrown to indicate that this method should not be directly used for parsing.
        /// The error is logged using <see cref="SapAddon.Logger"/>, and a default value
        /// of <see cref="DateTime.MinValue"/> is returned to indicate unsuccessful parsing.
        /// </exception>
        /// <seealso cref="DateToSapData"/>
        /// <seealso cref="TimeToSapData"/>
        public override object ParseValue(object value) {
            NotSupportedException ex =
                new NotSupportedException(
                    Texts.DateTimeFieldAttribute_ParseValue_ParseValue_should_not_be_used_on_DateTimeFieldAttribute__Use_the_specialized_handling_for_SAP_date_time_fields_);
            SapAddon.Instance().Logger.Error(ex);
            return DateTime.MinValue;
        }

        /// <inheritdoc />
        /// <remarks>
        /// ⚠️ This method is not intended for direct use with DateTime values in this class.
        /// Instead, use the specialized methods:
        /// <list type="bullet">
        ///     <item><description><see cref="DateToSapData"/> - For formatting the date portion</description></item>
        ///     <item><description><see cref="TimeToSapData"/> - For formatting the time portion</description></item>
        /// </list>
        /// This separation is necessary because DateTime values in SAP Business One are stored as two separate fields:
        /// one for the date (format: "yyyyMMdd") and one for the time (format: "HHmm").
        /// </remarks>
        public override string ToSapData(object value) {
            NotSupportedException ex =
                new NotSupportedException(
                    Texts.DateTimeFieldAttribute_ToSapData_ToSapData_should_not_be_used_on_DateTimeFieldAttribute__Use_the_specialized_handling_for_SAP_date_time_fields_);
            SapAddon.Instance().Logger.Error(ex);
            return "";
        }

        /// <inheritdoc />
        public override bool ValidateField(object value) => true;

        /// <inheritdoc />
        public override Type Type => typeof(DateTime?);

        /// <inheritdoc />
        DateTime? IUserTableField<DateTime?>.DefaultValue => _stronglyTypedDefaultValue;

        /// <inheritdoc />
        DateTime? IUserTableField<DateTime?>.ParseValue(object value) => (DateTime?)ParseValue(value);

        /// <summary>
        /// Converts the provided value into a date format.
        /// </summary>
        /// <param name="value">The object value to be parsed into a date.
        /// Expected to be compatible with <see cref="DateTime"/>. Null or invalid formats may throw an exception depending on the implementation.</param>
        /// <returns>The parsed date as a <see cref="DateTime"/> object.</returns>
        /// <remarks>
        /// This method uses the <see cref="Parsers.ParseDate"/> utility to handle the date parsing. It ensures consistent
        /// formatting when interpreting object values as date representations.
        /// </remarks>
        /// <seealso cref="Parsers.ParseDate"/>
        public DateTime ParseDateValue(object value) {
            return Parsers.ParseDate(value);
        }

        /// <summary>
        /// Parses the provided value into a time component of a <see cref="DateTime"/> object.
        /// </summary>
        /// <param name="value">The object value to be parsed into a time component.</param>
        /// <returns>An object representing the parsed time value as a <see cref="DateTime"/>.</returns>
        /// <remarks>
        /// This method utilizes the <see cref="Parsers.ParseTime"/> utility.
        /// </remarks>
        /// <seealso cref="Parsers.ParseTime"/>
        public DateTime ParseTimeValue(object value) {
            return Parsers.ParseTime(value);
        }

        /// <summary>
        /// Converts the given value into a SAP-compatible date string in the format "yyyyMMdd".
        /// </summary>
        /// <param name="value">
        /// The value to be converted. Accepts null, <see cref="DateTime"/>, or string values.
        /// If the <paramref name="value"/> is null, an empty string is returned.
        /// If the <paramref name="value"/> is a <see cref="DateTime"/>, it formats the date unless it's <see cref="DateTime.MinValue"/>.
        /// If the <paramref name="value"/> is a string, it attempts to parse and format it as a date in the "yyyyMMdd" format.
        /// </param>
        /// <returns>
        /// A string representing the SAP-compatible date if successful, or an empty string in case of invalid input or conversion failure.
        /// </returns>
        /// <remarks>
        /// This method is specifically designed to handle dates for SAP systems and ensures compatibility with their expected format.
        /// </remarks>
        /// <seealso cref="DateTimeFieldAttribute"/>
        /// <seealso cref="CultureInfo"/>
        [Localizable(false)]
        public string DateToSapData(object value) {
            switch (value) {
                case null:
                    return "";
                case DateTime dt:
                    return dt.Year < 1900 ? "" : dt.ToString("yyyyMMdd");
                case string str:
                {
                    return DateTime.TryParseExact(str, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed)
                        ? parsed.ToString("yyyyMMdd")
                        : "";
                }
                default:
                    return "";
            }
        }

        /// <summary>
        /// Converts a given <see cref="DateTime"/> value to its SAP-compatible time format representation.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> object to be converted. If null, an empty string will be returned.</param>
        /// <returns>A string representation of the time in the "HHmm" format.</returns>
        /// <seealso cref="DateTimeFieldAttribute" />
        [Localizable(false)]
        public string TimeToSapData(object value) {
            return value == null ? "" : ((DateTime)value).ToString("HHmm");
        }
        /// <summary>
        /// Converts a given date value to a representation suitable for SAP columns.
        /// </summary>
        /// <param name="value">The date value to be converted. Must be compatible with <see cref="DateTime"/>.</param>
        /// <returns>A string representing the formatted date value for SAP column storage.</returns>
        /// <seealso cref="DateTimeFieldAttribute.DateToSapData"/>
        public string DateToColumnData(object value) {
            return DateToSapData(value);
        }
        /// <summary>
        /// Converts a given object representing time to a string formatted for database column data representation.
        /// </summary>
        /// <param name="value">
        /// The object representing the time to be converted. It is expected to be of type DateTime or null.
        /// </param>
        /// <returns>
        /// A string formatted as "HH:mm" if the value is a valid DateTime object.
        /// If the value is null, "0000" is returned.
        /// </returns>
        [Localizable(false)]
        public string TimeToColumnData(object value) {
            return value == null ? "0000" : ((DateTime)value).ToString("HH:mm");
        }
    }
}