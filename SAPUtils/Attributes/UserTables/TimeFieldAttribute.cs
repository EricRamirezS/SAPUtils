using System;
using System.ComponentModel;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.Utils;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to mark a class property as a SAP Business One user-defined field (UDF) 
    /// specifically for storing time values.
    /// <br/>
    /// This attribute ensures that the field is stored as `db_Date` with the subtype `st_Time`, 
    /// allowing time-specific storage and formatting.
    /// <br/>
    /// <b>Usage:</b><br/>
    /// - Apply this attribute to properties in a user table object model to define a time field.<br/>
    /// - The time is stored as a `DateTime` value but formatted as `"HHmm"` in SAP.<br/>
    /// - Implements <see cref="IUserTableField{T}"/> for type-safe parsing and default value handling.<br/>
    /// <br/>
    /// <b>Example:</b>
    /// <code>
    /// [TimeUserTableField(Name = "StartTime", Description = "Event start time", Mandatory = true)]
    /// public DateTime? StartTime { get; set; }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class TimeFieldAttribute : UserTableFieldAttributeBase, IUserTableField<DateTime?> {
        private DateTime? _stronglyTypedDefaultValue;

        /// <inheritdoc />
        public override BoFieldTypes FieldType => BoFieldTypes.db_Date;

        /// <inheritdoc />
        public override BoFldSubTypes SubType => BoFldSubTypes.st_Time;

        /// <inheritdoc />
        public override int Size { get; set; } = 4;

        /// <inheritdoc />
        public override object DefaultValue
        {
            get => _stronglyTypedDefaultValue;
            set => _stronglyTypedDefaultValue = (DateTime?)ParseValue(value);
        }

        /// <inheritdoc />
        public override object ParseValue(object value) {
            return Parsers.ParseTime(value);
        }

        /// <inheritdoc />
        [Localizable(false)]
        public override string ToSapData(object value) {
            return value == null ? "" : ((DateTime)value).ToString("HHmm");
        }

        /// <inheritdoc />
        public override bool ValidateField(object value) => true;

        /// <inheritdoc />
        [Localizable(false)]
        public override string ToColumnData(object value) {
            return value == null ? "0000" : ((DateTime)value).ToString("HH:mm");
        }

        /// <inheritdoc />
        public override Type Type => typeof(DateTime?);

        /// <inheritdoc />
        DateTime? IUserTableField<DateTime?>.DefaultValue => _stronglyTypedDefaultValue;

        /// <inheritdoc />
        DateTime? IUserTableField<DateTime?>.ParseValue(object value) => (DateTime?)ParseValue(value);
    }
}