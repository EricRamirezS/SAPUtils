using System;
using System.Globalization;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to mark a class property as a SAP Business One user-defined field (UDF) 
    /// specifically for storing integer numeric values.
    /// <br/>
    /// This attribute ensures that the field is stored as `db_Numeric`, suitable for whole numbers, 
    /// with a default maximum size of 11 digits.
    /// <br/>
    /// <b>Usage:</b><br/>
    /// - Apply this attribute to properties in a user table object model to define a numeric field.<br/>
    /// - The default precision is based on the system's price accuracy.<br/>
    /// - Implements <see cref="IUserTableField{T}"/> for type-safe parsing and default value handling.<br/>
    /// <br/>
    /// <b>Example:</b>
    /// <code>
    /// [NumericUserTableField(Name = "Age", Description = "Customer age", Required = true)]
    /// public int? Age { get; set; }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NumericUserTableFieldAttribute : UserTableFieldAttributeBase, IUserTableField<int?> {
        private int? _stronglyTypedDefaultValue;

        /// <inheritdoc />
        public override BoFieldTypes FieldType => BoFieldTypes.db_Numeric;

        /// <inheritdoc />
        public override int Size { get; set; } = 11;

        /// <inheritdoc />
        public override object DefaultValue
        {
            get => _stronglyTypedDefaultValue;
            set => _stronglyTypedDefaultValue = (int?)ParseValue(value);
        }

        /// <inheritdoc/>
        public override object ParseValue(object value) {
            return int.TryParse(value?.ToString(), out int result) ? result : 0;
        }

        /// <inheritdoc />
        public override string ToSapData(object value) {
            return value == null ? "0" : ((int)value).ToString(CultureInfo.InvariantCulture);
        }

        /// <inheritdoc />
        public override Type Type => typeof(int);

        /// <inheritdoc />
        int? IUserTableField<int?>.DefaultValue => _stronglyTypedDefaultValue;

        /// <inheritdoc />
        int? IUserTableField<int?>.ParseValue(object value) => (int?)ParseValue(value);
    }
}