using System;
using SAPUtils.Attributes.UserTables;
using SAPUtils.I18N;

namespace SAPUtils.Exceptions {
    /// <summary>
    /// Represents an exception that is thrown when a Code is not provided for new data
    /// in a specific table while the PrimaryKeyStrategy is set to Manual.
    /// </summary>
    /// <remarks>
    /// The CodeNotSetException ensures that the Code property is explicitly provided
    /// during operations that rely on the Manual PrimaryKeyStrategy. This prevents
    /// invalid states where a unique identifier is missing for the new data.
    /// </remarks>
    /// <exception cref="CodeNotSetException">
    /// Thrown when no Code is provided for new records in a user table and the
    /// PrimaryKeyStrategy is set to Manual.
    /// </exception>
    public class CodeNotSetException : Exception {
        /// <summary>
        /// Thrown when no code is provided for a new data entry in a user table, and the
        /// <see cref="PrimaryKeyStrategy"/> is set to <see cref="PrimaryKeyStrategy.Manual"/>.
        /// </summary>
        /// <param name="tableName">
        /// The name of the table where the CodeNotSetException is thrown. This helps
        /// identify which table instance caused the exception.
        /// </param>
        /// <remarks>
        /// This exception is used to indicate that a unique code is required when the
        /// primary key strategy for the table is manually defined but has not been supplied.
        /// </remarks>
        public CodeNotSetException(string tableName) : base(
            string.Format(Texts.CodeNotSetException_CodeNotSetException_No_Code_was_provided_for_new_data_in__0___but_PrimaryKeyStrategy_was_set_to_Manual_, tableName)) { }
    }
}