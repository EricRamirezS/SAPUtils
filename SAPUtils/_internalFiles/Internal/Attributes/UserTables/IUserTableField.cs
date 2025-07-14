using System;
using System.Collections.Generic;
using SAPbobsCOM;
using SAPUtils.Models.UserTables;

namespace SAPUtils.Internal.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to mark a class property as a SAP Business One UserField.
    /// This interface defines the metadata and behavior required to handle user-defined fields (UDFs)
    /// in SAP Business One through the DI API.
    ///
    /// The corresponding SAP Business One object is <see cref="UserFieldsMD"/>, which enables:
    /// - Adding user-defined fields to system and user tables.
    /// - Retrieving user-defined fields from the database.
    /// - Removing user-defined fields.
    /// - Saving the object in XML format.
    ///
    /// **Important:** 
    /// - After creating a new user-defined field, you must release the `UserFieldsMD` object using:
    ///   `System.Runtime.InteropServices.Marshal.ReleaseComObject(myObject);`
    /// - If a mandatory field is added, a default value must be provided; otherwise, an error will occur.
    ///
    /// **Source Table:** CUFD
    /// </summary>
    internal interface IUserTableField {
        /// <summary>
        /// Gets or sets the technical name of the user-defined field (UDF).
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the user-defined field.
        /// This is the human-readable label displayed in SAP Business One.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets the data type of the field, as defined in SAP Business One.
        /// </summary>
        BoFieldTypes FieldType { get; }

        /// <summary>
        /// Gets the subtype of the field, used for additional type restrictions.
        /// </summary>
        BoFldSubTypes SubType { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the field is mandatory.
        /// If `true`, a default value must be assigned before adding the field.
        /// </summary>
        bool Required { get; set; }

        /// <summary>
        /// Gets or sets the maximum length of the field.
        /// </summary>
        int Size { get; set; }

        /// <summary>
        /// Gets or sets the default value of the field, used when no value is assigned.
        /// </summary>
        object DefaultValue { get; set; }

        /// <summary>
        /// Gets a list of valid values for the field.
        /// Used for fields with predefined value sets.
        /// </summary>
        IList<IUserFieldValidValue> ValidValues { get; }
        
        /// <summary>
        /// Gets or sets a raw string array of valid values for the field.
        /// Used for fields with predefined value sets.
        /// </summary>
        string[] ValidValuePairs { get; set; }

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
        /// Gets the .NET type that corresponds to this user-defined field.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Gets or sets the SAP Business One system object type this field is linked to, if any.
        /// Used when the user-defined field should reference a system object such as Business Partners or Items.
        /// </summary>
        UDFLinkedSystemObjectTypesEnum? LinkedSystemObject { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the user-defined table that this field is linked to, if applicable.
        /// This enables dropdowns or search helps based on user-defined tables.
        /// </summary>
        string LinkedTable { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the user-defined table that this field is linked to, if applicable.
        /// This enables dropdowns or search helps based on user-defined tables.
        /// </summary>
        Type LinkedTableType { get; set; }
        
        /// <summary>
        /// Gets or sets the identifier of the User Defined Object (UDO) that this field is linked to, if any.
        /// Useful when the field should point to a UDO instead of a raw table.
        /// </summary>
        string LinkedUdo { get; set; }
    }
}