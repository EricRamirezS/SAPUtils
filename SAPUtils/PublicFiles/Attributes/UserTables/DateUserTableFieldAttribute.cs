using System;
using System.Globalization;
using SAPbobsCOM;
using SAPUtils.Internal.Attributes.UserTables;

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to mark a class property as a SAP Business One user-defined field (UDF) 
    /// that stores both a date and a time as separate fields.
    /// <br/>
    /// This attribute creates two UDFs in SAP Business One:<br/>
    /// - A field of type (`db_Date`) with subtype (`st_None`), named `{Name}Date`.<br/>
    /// - A field of type (`db_Date`) with subtype (`st_Time`), named `{Name}Time`.<br/>
    /// <br/>
    /// <b>Usage:</b><br/>
    /// - Apply this attribute to properties in a user table object model to define a date/time pair.<br/>
    /// - Implements <see cref="IUserTableField{T}"/> for type-safe parsing and default value handling.<br/>
    /// <br/>
    /// <b>Example:</b>
    /// <code>
    /// [DateTimeUserTableField(Name = "Event", Description = "Event date and time", Required = true)]
    /// public DateTime? EventDateTime { get; set; }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DateUserTableFieldAttribute : UserTableFieldAttributeBase, IUserTableField<DateTime?> {
        private DateTime? _stronglyTypedDefaultValue;

        /// <inheritdoc />
        public override BoFieldTypes FieldType => BoFieldTypes.db_Date;

        /// <inheritdoc />
        public override int Size { get; set; } = 8;

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
                    "yyyyMMdd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime result) || DateTime.TryParse(value?.ToString(), out result))
                return result;

            return null;
        }

        /// <inheritdoc />
        public override string ToSapData(object value) {
            return ((DateTime)value).ToString("yyyyMMdd");
        }

        /// <inheritdoc />
        public override Type Type => typeof(DateTime?);

        /// <inheritdoc />
        DateTime? IUserTableField<DateTime?>.DefaultValue => _stronglyTypedDefaultValue;

        /// <inheritdoc />
        DateTime? IUserTableField<DateTime?>.ParseValue(object value) => (DateTime?)ParseValue(value);
    }
}