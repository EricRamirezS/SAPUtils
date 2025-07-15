using System;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to mark a class property as a SAP Business One user-defined field (UDF) 
    /// specifically for storing phone numbers.
    /// <br/>
    /// This attribute ensures that the field is stored as `db_Alpha` with the subtype `st_Phone`, 
    /// providing appropriate formatting for phone numbers.
    /// <br/>
    /// <b>Usage:</b><br/>
    /// - Apply this attribute to properties in a user table object model to define a phone number field.<br/>
    /// - Implements <see cref="IUserTableField{T}"/> for type-safe parsing and default value handling.<br/>
    /// <br/>
    /// <b>Example:</b>
    /// <code>
    /// [PhoneUserTableField(Name = "CustomerPhone", Description = "Primary contact phone number", Required = true)]
    /// public string CustomerPhone { get; set; }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PhoneUserTableFieldAttribute : UserTableFieldAttributeBase, IUserTableField<string> {
        private string _stronglyTypedDefaultValue = string.Empty;

        /// <inheritdoc />
        public override BoFieldTypes FieldType => BoFieldTypes.db_Alpha;

        /// <inheritdoc />
        public override BoFldSubTypes SubType => BoFldSubTypes.st_Phone;

        /// <inheritdoc />
        public override int Size { get; set; } = 20;


        /// <inheritdoc />
        public override object DefaultValue
        {
            get => _stronglyTypedDefaultValue;
            set => _stronglyTypedDefaultValue = (string)ParseValue(value);
        }

        /// <inheritdoc />
        public override object ParseValue(object value) {
            return value?.ToString() ?? _stronglyTypedDefaultValue ?? string.Empty;
        }

        /// <inheritdoc />
        public override string ToSapData(object value) {
            return value?.ToString() ?? string.Empty;
        }

        /// <inheritdoc />
        public override Type Type => typeof(string);

        string IUserTableField<string>.DefaultValue => _stronglyTypedDefaultValue;

        string IUserTableField<string>.ParseValue(object value) => Convert.ToString(ParseValue(value));
    }
}