using System;
using SAPUtils.I18N;

namespace SAPUtils.Exceptions {
    /// <summary>
    /// Represents an exception that is thrown when a class is expected to have a
    /// <c>UserTableAttribute</c>, but the attribute is not found.
    /// </summary>
    /// <remarks>
    /// This exception is typically used in scenarios where metadata validation is performed
    /// on user-defined table models. The absence of the required attribute results
    /// in this exception being thrown to signal an invalid model configuration.
    /// </remarks>
    /// <seealso cref="SAPUtils.Models.UserTables.UserTableObjectModel{T}"/>
    public class UserTableAttributeNotFound : Exception {
        /// <summary>
        /// Exception thrown when a class is expected to have a <see cref="SAPUtils.Attributes.UserTables.UserTableAttribute"/>
        /// but is not decorated with it.
        /// </summary>
        /// <param name="name">
        /// The name of the class that is missing the required <see cref="SAPUtils.Attributes.UserTables.UserTableAttribute"/>.
        /// </param>
        /// <remarks>
        /// This exception is typically thrown during the initialization of a user table object model if the model class is
        /// not properly annotated with the required attribute.
        /// </remarks>
        /// <seealso cref="SAPUtils.Attributes.UserTables.UserTableAttribute"/>
        public UserTableAttributeNotFound(string name) : base(string.Format(Texts.UserTableAttributeNotFound_UserTableAttributeNotFound__0__is_not_decorated_with_UserTableAttribute_, name)) { }
    }

}