using System;
using System.Globalization;
using SAPbobsCOM;
using SAPUtils.Internal.Attributes.UserTables;

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to mark a class property as a SAP Business One user-defined field (UDF) 
    /// specifically for storing percentage values.
    /// <br/>
    /// This attribute ensures that the field is stored as `db_Float` with the subtype `st_Percentage`, 
    /// allowing accurate representation of percentage-based values.
    /// <br/>
    /// <b>Usage:</b><br/>
    /// - Apply this attribute to properties in a user table object model to define a percentage field.<br/>
    /// - The default precision is based on the system's price accuracy.<br/>
    /// - Implements <see cref="IUserTableField{T}"/> for type-safe parsing and default value handling.<br/>
    /// <br/>
    /// <b>Example:</b>
    /// <code>
    /// [PercentageUserTableField(Name = "DiscountRate", Description = "Customer discount percentage", Required = true)]
    /// public double? DiscountRate { get; set; }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PercentageUserTableFieldAttribute : UserTableFieldAttributeBase, IUserTableField<double?> {
        private double? _stronglyTypedDefaultValue;

        /// <inheritdoc />
        public override BoFieldTypes FieldType => BoFieldTypes.db_Numeric;

        /// <inheritdoc />
        public override int Size { get; set; } = 11;

        /// <inheritdoc />
        public override object DefaultValue
        {
            get => _stronglyTypedDefaultValue;
            set => _stronglyTypedDefaultValue = (double?)ParseValue(value);
        }

        /// <inheritdoc/>
        public override object ParseValue(object value) {
            return double.TryParse(value?.ToString(), out double result) ? result : 0;
        }

        /// <inheritdoc />
        public override string ToSapData(object value) {
            return value == null ? "0" : ((int)value).ToString(CultureInfo.InvariantCulture);
        }

        /// <inheritdoc />
        public override Type Type => typeof(int);

        /// <inheritdoc />
        double? IUserTableField<double?>.DefaultValue => _stronglyTypedDefaultValue;

        /// <inheritdoc />
        double? IUserTableField<double?>.ParseValue(object value) => (int?)ParseValue(value);
    }
}