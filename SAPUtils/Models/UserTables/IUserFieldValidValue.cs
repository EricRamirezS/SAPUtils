namespace SAPUtils.Models.UserTables {
    /// <summary>
    /// Represents a valid value for a user-defined field, including both its internal value and a user-friendly description.
    /// </summary>
    public interface IUserFieldValidValue {
        /// <summary>
        /// Gets the internal value of the user field.
        /// This is typically the value stored in the database or used for logic comparisons.
        /// </summary>
        string Value { get; }

        /// <summary>
        /// Gets the human-readable description associated with the value.
        /// This is usually displayed in the UI to represent the meaning of the <see cref="Value"/>.
        /// </summary>
        string Description { get; }
    }
}