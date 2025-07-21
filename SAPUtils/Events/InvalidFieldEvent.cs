using System.Reflection;
using SAPUtils.Attributes.UserTables;

namespace SAPUtils.Events {
    /// <summary>
    /// Represents an event that is triggered when an invalid field is encountered
    /// within the context of user-defined field processing for SAP.
    /// </summary>
    /// <remarks>
    /// The <c>InvalidFieldEvent</c> class defines a delegate and an associated
    /// event to signal invalid field encounters with the parameters necessary to
    /// identify the affected field.
    /// </remarks>
    public class InvalidFieldEvent {
        /// <summary>
        /// Represents the delegate used to handle invalid field events in the context of SAP utilities.
        /// </summary>
        /// <param name="property">The <see cref="System.Reflection.PropertyInfo"/> associated with the invalid field.</param>
        /// <param name="userField">The <see cref="IUserField"/> that is deemed invalid or generates the event.</param>
        /// <remarks>
        /// This delegate is invoked when an invalid field event is triggered, allowing the handling of errors
        /// or anomalies related to specific user-defined fields.
        /// </remarks>
        /// <seealso cref="T:SAPUtils.Events.InvalidFieldEvent"/>
        /// <seealso cref="IUserField"/>
        public delegate void InvalidFieldHandler(PropertyInfo property, IUserField userField);

        /// <summary>
        /// Represents the event triggered when an invalid field operation is encountered.
        /// </summary>
        /// <remarks>
        /// This event is used to handle scenarios where a property associated with a user-defined field
        /// does not meet expected criteria or encounters validation errors.
        /// </remarks>
        /// <seealso cref="IUserField" />
        public static event InvalidFieldHandler InvalidField;

        internal static void Invoke(PropertyInfo property, IUserField userField) {
            InvalidField?.Invoke(property, userField);
        }
    }
}