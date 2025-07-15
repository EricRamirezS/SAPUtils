using System;
using System.Globalization;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to mark a class property as a SAP Business One user-defined field (UDF) 
    /// specifically for storing price values.
    /// <br/>
    /// This attribute ensures that the field is stored as `db_Float` with the subtype `st_Price`, 
    /// applying the system's price accuracy settings.
    /// <br/>
    /// <b>Usage:</b><br/>
    /// - Apply this attribute to properties in a user table object model to define a price field.<br/>
    /// - The default precision is based on the system's price accuracy.<br/>
    /// - Implements <see cref="IUserTableField{T}"/> for type-safe parsing and default value handling.<br/>
    /// <br/>
    /// <b>Example:</b>
    /// <code>
    /// [PriceUserTableField(Name = "ProductPrice", Description = "Product sale price", Required = true)]
    /// public double? ProductPrice { get; set; }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PriceUserTableFieldAttribute : UserTableFieldAttributeBase, IUserTableField<double?> {
        private double? _stronglyTypedDefaultValue;

        /// <inheritdoc />
        public override BoFieldTypes FieldType => BoFieldTypes.db_Float;

        /// <inheritdoc />
        public override BoFldSubTypes SubType => BoFldSubTypes.st_Price;

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