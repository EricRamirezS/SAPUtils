using System;
using System.Globalization;
using SAPbobsCOM;
using SAPUtils.Internal.Attributes.UserTables;

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to mark a class property as a SAP Business One user-defined field (UDF) 
    /// specifically for storing measurement values.
    /// <br/>
    /// This attribute ensures that the field is stored as a floating-point number (`db_Float`) with 
    /// a subtype of `st_Measurement`, maintaining consistency with SAP Business One's measurement accuracy.
    /// <br/>
    /// <b>Usage:</b><br/>
    /// - Apply this attribute to properties in a user table object model to define a measurement field.<br/>
    /// - The default precision is based on the system's price accuracy.<br/>
    /// - Implements <see cref="IUserTableField{T}"/> for type-safe parsing and default value handling.<br/>
    /// - Values are stored in a culture-invariant format.<br/>
    /// <br/>
    /// <b>Example:</b>
    /// <code>
    /// [MeasurementUserTableField(Name = "ItemWeight", Description = "Weight of the item in kg", Required = true)]
    /// public double? ItemWeight { get; set; }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MeasurementUserTableFieldAttribute : UserTableFieldAttributeBase, IUserTableField<double?> {
        private double? _stronglyTypedDefaultValue;

        /// <inheritdoc />
        public override int Size { get; set; } = 11;

        /// <inheritdoc />
        public override BoFieldTypes FieldType => BoFieldTypes.db_Float;

        /// <inheritdoc />
        public override BoFldSubTypes SubType => BoFldSubTypes.st_Measurement;

        /// <inheritdoc />
        public override object DefaultValue
        {
            get => _stronglyTypedDefaultValue;
            set => _stronglyTypedDefaultValue = (double?)ParseValue(value);
        }

        /// <inheritdoc />
        public override object ParseValue(object value) {
            return double.TryParse(value?.ToString(), out double result) ? result : 0;
        }

        /// <inheritdoc />
        public override string ToSapData(object value) {
            return value == null ? "0" : ((double)value).ToString(CultureInfo.InvariantCulture);
        }

        /// <inheritdoc />
        public override Type Type => typeof(double);

        /// <inheritdoc />
        double? IUserTableField<double?>.DefaultValue => _stronglyTypedDefaultValue;

        /// <inheritdoc />
        double? IUserTableField<double?>.ParseValue(object value) => (double?)ParseValue(value);
    }
}