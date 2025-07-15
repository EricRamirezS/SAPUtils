using System;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to mark a class property as a SAP Business One user-defined field (UDF) 
    /// for storing alphanumeric data.
    /// <br/>
    /// This attribute is designed for fields that store string values in SAP Business One, ensuring the 
    /// correct field type (`db_Alpha`) and default subtype (`st_None`).
    /// <br/>
    /// <b>Usage:</b><br/>
    /// - Apply this attribute to properties in a user table object model to define an alphanumeric field.<br/>
    /// - The default maximum size is <b>50 characters</b>.<br/>
    /// - Implements <see cref="IUserTableField{T}"/> for type-safe parsing and default value handling.<br/>
    /// <br/>
    /// <b>Example:</b>
    /// <code>
    /// [AlphaUserTableField(Name = "CustomerCode", Description = "Unique Customer Identifier", Required = true)]
    /// public string CustomerCode { get; set; }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class AlphaUserTableFieldAttribute : UserTableFieldAttributeBase, IUserTableField<string> {

        private string _stronglyTypedDefaultValue = string.Empty;

        /// <inheritdoc />
        public override BoFieldTypes FieldType => BoFieldTypes.db_Alpha;

        /// <inheritdoc />
        public override int Size { get; set; } = 100;

        /// <inheritdoc />
        public override object DefaultValue
        {
            get => _stronglyTypedDefaultValue;
            set => _stronglyTypedDefaultValue = (string)ParseValue(value);
        }

        /// <inheritdoc />
        public override object ParseValue(object value) {
            return value?.ToString() ?? _stronglyTypedDefaultValue ?? "";
        }

        /// <inheritdoc />
        public override string ToSapData(object value) {
            return value?.ToString() ?? "";
        }

        /// <inheritdoc />
        public override Type Type => typeof(string);

        string IUserTableField<string>.DefaultValue => _stronglyTypedDefaultValue;

        string IUserTableField<string>.ParseValue(object value) => Convert.ToString(ParseValue(value));
    }
}