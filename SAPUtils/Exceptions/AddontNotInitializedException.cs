using System;
using SAPUtils.I18N;

// ReSharper disable UnusedType.Global

namespace SAPUtils.Exceptions {
    /// <summary>
    /// Represents an exception that is thrown when an attempt is made to use an add-on
    /// that has not been properly initialized.
    /// </summary>
    /// <remarks>
    /// This exception is specifically used to indicate that the required initialization process
    /// for an add-on has not been completed before attempting to execute its functionality.
    /// </remarks>
    /// <seealso cref="System.Exception"/>
    public class AddontNotInitializedException : Exception {
        /// <summary>
        /// Represents an exception that is thrown when an attempt is made to use an add-on
        /// that has not been properly initialized.
        /// </summary>
        public AddontNotInitializedException() : base(
            Texts.AddontNotInitializedException_AddontNotInitializedException_The_add_on_has_not_been_initialized_) { }
    }
}