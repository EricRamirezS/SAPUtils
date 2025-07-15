using System;
using System.Globalization;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;

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
    /// [DateTimeUserTableField(Name = "OrderDate", Description = "The date when the order was placed", Required = true)]
    /// public DateTime? OrderDate { get; set; }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DateTimeUserTableFieldAttribute : UserTableFieldAttributeBase, IUserTableField<DateTime?> {

        private string _dateDescription;
        private DateTime? _stronglyTypedDefaultValue;
        private string _timeDescription;

        /// <summary>
        /// Gets or sets the description used for the date portion of this field in SAP Business One.
        /// <br/>
        /// <b>Important:</b> Unlike other field types, <see cref="DateTimeUserTableFieldAttribute"/> maps to 
        /// <b>two separate columns</b> in the database — one for the date and one for the time.
        /// This property allows customizing the human-readable label of the <b>date column</b>.
        /// <br/>
        /// If not explicitly set, it falls back to <see cref="UserTableFieldAttributeBase.Description"/> or defaults to <c>{Name}Date</c>.
        /// </summary>
        public string DateDescription
        {
            get => _dateDescription ?? (Description ?? $"{Name}Date");
            set => _dateDescription = value;
        }

        /// <summary>
        /// Gets or sets the description used for the time portion of this field in SAP Business One.
        /// <br/>
        /// <b>Important:</b> Unlike other field types, <see cref="DateTimeUserTableFieldAttribute"/> maps to 
        /// <b>two separate columns</b> in the database — one for the date and one for the time.
        /// This property allows customizing the human-readable label of the <b>time column</b>.
        /// <br/>
        /// If not explicitly set, it falls back to <see cref="UserTableFieldAttributeBase.Description"/> or defaults to <c>{Name}Time</c>.
        /// </summary>
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
            set => _stronglyTypedDefaultValue = (DateTime?)ParseValue(value);
        }

        /// <inheritdoc />
        public override object ParseValue(object value) {
            if (value is DateTime dt)
                return dt;

            if (DateTime.TryParseExact(
                    value?.ToString(),
                    "yyyyMMddHHmmss",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime result) || DateTime.TryParse(value?.ToString(), out result))
                return result;

            return null;
        }

        /// <inheritdoc />
        public override string ToSapData(object value) {
            // Tratamiento Especial
            return null;
        }

        /// <inheritdoc />
        public override Type Type => typeof(DateTime?);

        /// <inheritdoc />
        DateTime? IUserTableField<DateTime?>.DefaultValue => _stronglyTypedDefaultValue;

        /// <inheritdoc />
        DateTime? IUserTableField<DateTime?>.ParseValue(object value) => (DateTime?)ParseValue(value);
    }
}