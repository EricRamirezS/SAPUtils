using System;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to mark a class property as a SAP Business One user-defined field (UDF) 
    /// for storing boolean values.
    /// <br/>
    /// This attribute is designed for fields that store boolean (true/false) values in SAP Business One, ensuring 
    /// the correct field type (`db_Numeric`) and default subtype (`st_None`).
    /// <br/>
    /// <b>Usage:</b><br/>
    /// - Apply this attribute to properties in a user table object model to define a boolean field.<br/>
    /// - The field is stored as an integer (`1` for true, `0` for false).<br/>
    /// - Implements <see cref="IUserTableField{T}"/> for type-safe parsing and default value handling.<br/>
    /// <br/>
    /// <b>Example:</b>
    /// <code>
    /// [BooleanUserTableField(Name = "IsActive", Description = "Indicates if the customer is active", Required = true)]
    /// public bool? IsActive { get; set; }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class BooleanUserTableFieldAttribute : UserTableFieldAttributeBase, IUserTableField<bool?> {
        private bool? _stronglyTypedDefaultValue;

        /// <inheritdoc />
        public override BoFieldTypes FieldType => BoFieldTypes.db_Alpha;

        /// <inheritdoc />
        public override int Size { get; set; } = 1;

        /// <inheritdoc />
        public override object DefaultValue
        {
            get => _stronglyTypedDefaultValue;
            set => _stronglyTypedDefaultValue = (bool?)ParseValue(value);
        }

        /// <inheritdoc />
        public override object ParseValue(object value) {
            switch (value) {
                case null:
                    return null;
                case string s:
                    return s == "Y";
                case bool b:
                    return b;
                case int i:
                    return i != 0;
                default:
                    return bool.TryParse(value.ToString(), out bool result) ? result : (object)null;
            }
        }


        /// <inheritdoc />
        public override string ToSapData(object value) {
            if (value == null) return "N";
            return (bool)value ? "Y" : "N";
        }

        /// <inheritdoc />
        public override Type Type => typeof(bool?);

        /// <inheritdoc />
        bool? IUserTableField<bool?>.DefaultValue => _stronglyTypedDefaultValue;

        /// <inheritdoc />
        bool? IUserTableField<bool?>.ParseValue(object value) => (bool?)ParseValue(value);
    }
}