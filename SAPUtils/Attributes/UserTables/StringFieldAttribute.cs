using System;
using System.Linq;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents a base attribute to define text properties associated with user tables in SAP.
    /// </summary>
    /// <remarks>
    /// This attribute defines that the values of the properties to which it is applied will be treated as text types 
    /// (Alpha) within the user tables in SAP. It also establishes the default size and manages the value 
    /// predetermined.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class StringFieldAttribute : UserTableFieldAttributeBase, IUserTableField<string> {

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

        /// <inheritdoc />
        public override bool ValidateField(object value) {
            string s = ParseValue(value)?.ToString();
            if (s == null) {
                return !Mandatory;
            }
            if (s.Length > Size) {
                return false;
            }
            if (ValidValues == null || !ValidValues.Any()) return true;
            return ValidValues.Any(e => e.Value == s);
        }
    }
}