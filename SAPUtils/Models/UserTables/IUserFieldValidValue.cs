using SAPbobsCOM;

namespace SAPUtils.Models.UserTables {
    /// <summary>
    /// Represents a valid value for a user-defined field, including both its internal value and a user-friendly description.
    /// </summary>
    public interface IUserFieldValidValue : IValidValue, SAPbouiCOM.IValidValue { }

    /// <summary>
    /// Encapsulates a valid value for a user-defined field, consisting of a value and its corresponding description.
    /// </summary>
    public class ValidValue : IValidValue, SAPbouiCOM.IValidValue {
        /// <summary>
        /// Represents a valid value for a user-defined field, including both its internal value and a user-friendly description.
        /// </summary>
        /// <param name="value">The internal value representing the valid option.</param>
        /// <param name="description">A user-friendly description of the valid value.</param>
        /// <seealso cref="SAPUtils.Models.UserTables.IUserFieldValidValue"/>
        public ValidValue(string value, string description) {
            Value = value;
            Description = description;
        }

        /// <summary>
        /// Gets or sets the internal value of the user field.
        /// This is typically the value stored in the database or used for logic comparisons.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets or sets the human-readable description associated with the value.
        /// This is usually displayed in the UI to represent the meaning of the <see cref="Value"/>.
        /// </summary>
        public string Description { get; }
    }
}