using System.Collections.Generic;
using SAPbobsCOM;
using IValidValue = SAPbouiCOM.IValidValue;

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents a definition of a user field in the context of SAP utilities.
    /// </summary>
    /// <remarks>
    /// This interface is used for defining metadata related to user-defined fields
    /// such as their name, type, size, and associated validations.
    /// </remarks>
    /// <seealso cref="T:SAPUtils.__Internal.Attributes.UserTables.IUserTableField" />
    /// <seealso cref="T:SAPUtils.Attributes.UserTables.UserTableFieldAttributeBase" />
    public interface IUserField {
        /// <summary>
        /// Gets the technical name of the user-defined field (UDF).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the description of the user-defined field.
        /// This is the human-readable label displayed in SAP Business One.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the data type of the field, as defined in SAP Business One.
        /// </summary>
        BoFieldTypes FieldType { get; }

        /// <summary>
        /// Gets the subtype of the field, used for additional type restrictions.
        /// </summary>
        BoFldSubTypes SubType { get; }

        /// <summary>
        /// Gets or sets the maximum length of the field.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the field is mandatory.
        /// If `true`, a default value must be assigned before adding the field.
        /// </summary>
        bool Mandatory { get; }

        /// <summary>
        /// Gets or sets the SAP Business One system object type this field is linked to, if any.
        /// Used when the user-defined field should reference a system object such as Business Partners or Items.
        /// </summary>
        UDFLinkedSystemObjectTypesEnum LinkedSystemObject { get; }

        /// <summary>
        /// Gets or sets the name of the user-defined table that this field is linked to, if applicable.
        /// This enables dropdowns or search helps based on user-defined tables.
        /// </summary>
        string LinkedTable { get; }

        /// <summary>
        /// Gets or sets the identifier of the User Defined Object (UDO) that this field is linked to, if any.
        /// Useful when the field should point to a UDO instead of a raw table.
        /// </summary>
        string LinkedUdo { get; }

        /// <summary>
        /// Gets or sets the default value of the field, used when no value is assigned.
        /// </summary>
        object DefaultValue { get; }

        /// <summary>
        /// Gets a list of valid values for the field.
        /// Used for fields with predefined value sets.
        /// </summary>
        IList<IValidValue> ValidValues { get; }

        /// <summary>
        /// Parses a given object value into a format compatible with the SAP Business One field type.
        /// </summary>
        /// <param name="value">The raw value to be parsed.</param>
        /// <returns>The parsed value, formatted according to SAP standards.</returns>
        object ParseValue(object value);

        /// <summary>
        /// Converts a value to the format required by SAP Business One for storage.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <returns>A string representation of the value in SAP format.</returns>
        string ToSapData(object value);

        /// <summary>
        /// Validates the specified value against the constraints and rules defined for the user field.
        /// </summary>
        /// <param name="value">The value to be validated.</param>
        /// <returns><c>true</c> if the value is valid according to the field's constraints; otherwise, <c>false</c>.</returns>
        /// <seealso cref="T:SAPUtils.Models.UserTables.IUserFieldValidValue" />
        bool ValidateField(object value);
    }
}