using System;
using SAPbobsCOM;
using SAPUtils.Internal.Attributes.UserTables;

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to mark a class property as a SAP Business One user-defined field (UDF) 
    /// specifically for storing large text data.
    /// <br/>
    /// This attribute ensures that the field is stored as a `db_Memo`, allowing for long text storage without 
    /// a predefined length limit.
    /// <br/>
    /// <b>Usage:</b><br/>
    /// - Apply this attribute to properties in a user table object model to define a memo (large text) field.<br/>
    /// - Implements <see cref="IUserTableField{T}"/> for type-safe parsing and default value handling.<br/>
    /// - The default maximum size is set to <see cref="int.MaxValue"/>.<br/>
    /// <br/>
    /// <b>Example:</b>
    /// <code>
    /// [MemoUserTableField(Name = "Comments", Description = "Detailed customer feedback")]
    /// public string Comments { get; set; }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MemoUserTableFieldAttribute : UserTableFieldAttributeBase, IUserTableField<string> {
        private string _stronglyTypedDefaultValue;

        /// <inheritdoc />
        public override BoFieldTypes FieldType => BoFieldTypes.db_Memo;

        /// <inheritdoc />
        public override int Size { get; set; } = int.MaxValue;

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
            return value == null ? string.Empty : value.ToString();
        }

        /// <inheritdoc />
        public override Type Type => typeof(string);

        /// <inheritdoc />
        string IUserTableField<string>.DefaultValue => _stronglyTypedDefaultValue;

        /// <inheritdoc />
        string IUserTableField<string>.ParseValue(object value) => (string)ParseValue(value);
    }
}