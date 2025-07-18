using System.Collections.Generic;
using SAPbobsCOM;
using SAPUtils.Models.UserTables;

namespace SAPUtils.__Internal.Attributes.UserTables {
    /// <summary>
    /// Represents a definition of a user field in the context of SAP utilities.
    /// </summary>
    /// <remarks>
    /// This interface is used for defining metadata related to user-defined fields
    /// such as their name, type, size, and associated validations.
    /// </remarks>
    /// <seealso cref="T:SAPUtils.__Internal.Attributes.UserTables.IUserTableField" />
    /// <seealso cref="T:SAPUtils.Attributes.UserTables.UserTableFieldAttributeBase" />
    /// <seealso cref="T:SAPUtils.__Internal.Attributes.UserTables.UserField" />
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
        UDFLinkedSystemObjectTypesEnum? LinkedSystemObject { get; }

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
        IList<IUserFieldValidValue> ValidValues { get; }
    }

    /// <summary>
    /// Represents a definition of a user-defined field in the context of SAP Business One.
    /// </summary>
    /// <remarks>
    /// This class provides the structure and metadata to define custom fields
    /// along with their properties such as name, description, type, size, and validations.
    /// </remarks>
    /// <seealso cref="T:SAPUtils.__Internal.Attributes.UserTables.IUserField" />
    /// <seealso cref="T:SAPUtils.Models.UserTables.IUserFieldValidValue" />
    public class UserField : IUserField {

        /// <summary>
        /// Represents a user-defined field in the context of SAP Business One.
        /// </summary>
        /// <remarks>
        /// The <c>UserField</c> class provides metadata and structure used for defining custom fields in SAP Business One.
        /// It includes properties such as field name, description, type, size, mandatory status, default values, and valid value definitions.
        /// </remarks>
        /// <seealso cref="T:SAPUtils.__Internal.Attributes.UserTables.IUserField"/>
        /// <seealso cref="T:SAPUtils.Models.UserTables.IUserFieldValidValue"/>
        public UserField(string name,
            BoFieldTypes fieldType,
            string description = null,
            BoFldSubTypes subType = BoFldSubTypes.st_None,
            int size = 11,
            bool mandatory = false,
            UDFLinkedSystemObjectTypesEnum? linkedSystemObject = null,
            string linkedTable = null,
            string linkedUdo = null,
            object defaultValue = null,
            IList<IUserFieldValidValue> validValues = null) {
            Name = name;
            Description = description;
            FieldType = fieldType;
            SubType = subType;
            Size = size;
            Mandatory = mandatory;
            LinkedSystemObject = linkedSystemObject;
            LinkedTable = linkedTable;
            LinkedUdo = linkedUdo;
            DefaultValue = defaultValue;
            ValidValues = validValues;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Description { get; }

        /// <inheritdoc />
        public BoFieldTypes FieldType { get; }

        /// <inheritdoc />
        public BoFldSubTypes SubType { get; }

        /// <inheritdoc />
        public int Size { get; }

        /// <inheritdoc />
        public bool Mandatory { get; }

        /// <inheritdoc />
        public UDFLinkedSystemObjectTypesEnum? LinkedSystemObject { get; }

        /// <inheritdoc />
        public string LinkedTable { get; }

        /// <inheritdoc />
        public string LinkedUdo { get; }

        /// <inheritdoc />
        public object DefaultValue { get; }

        /// <inheritdoc />
        public IList<IUserFieldValidValue> ValidValues { get; }
    }
}