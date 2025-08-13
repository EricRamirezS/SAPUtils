using System;

namespace SAPUtils.Exceptions {
    /// <summary>
    /// Represents an exception thrown when a description exceeds the allowed length constraint.
    /// </summary>
    /// <remarks>
    /// This exception is commonly used in scenarios where a description field is validated to ensure it adheres
    /// to a predefined maximum length. Exceeding this length is considered a validation failure,
    /// prompting this exception to be thrown.
    /// </remarks>
    public class DescriptionTooLongException : Exception {
        /// <summary>
        /// Represents an exception thrown when a description exceeds the allowed length constraint.
        /// </summary>
        /// <remarks>
        /// This exception is commonly used in scenarios where a description field is validated to ensure it adheres
        /// to a predefined maximum length. Exceeding this length is considered a validation failure,
        /// prompting this exception to be thrown.
        /// </remarks>
        public DescriptionTooLongException(string message) : base(message) { }
    }
}