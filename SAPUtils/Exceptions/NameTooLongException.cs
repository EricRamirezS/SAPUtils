using System;

namespace SAPUtils.Exceptions {
    /// <summary>
    /// Represents an exception thrown when a provided name exceeds the allowed length constraint.
    /// </summary>
    /// <remarks>
    /// This exception is typically used in scenarios where a name field is validated to ensure it adheres
    /// to a predefined maximum length, and any name that surpasses this length is considered invalid.
    /// </remarks>
    public class NameTooLongException : Exception {
        /// <summary>
        /// Represents an exception that is thrown when a provided name exceeds the allowable length constraint.
        /// </summary>
        /// <remarks>
        /// This exception is commonly used to enforce validation for name fields that must adhere to a specific maximum length,
        /// ensuring the names remain within acceptable bounds for the system.
        /// </remarks>
        public NameTooLongException(string message) : base(message) { }
    }
}