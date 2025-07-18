using System;
using System.Runtime.InteropServices;

// ReSharper disable UnusedMember.Global

namespace SAPUtils.__Internal.Attributes.UserTables {
    /// <summary>
    /// Attribute used to mark a class property as a SAP Business One user-defined field (UDF).
    /// This interface defines the metadata and behavior necessary to handle UDFs through the SAP Business One DI API.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The corresponding SAP Business One object is <see cref="SAPbobsCOM.UserFieldsMD"/>, which allows:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Adding user-defined fields to both system and user tables.</description></item>
    /// <item><description>Retrieving user-defined fields from the database.</description></item>
    /// <item><description>Removing user-defined fields.</description></item>
    /// <item><description>Saving the object in XML format.</description></item>
    /// </list>
    /// <para><b>Important:</b></para>
    /// <list type="bullet">
    /// <item><description>After creating a new user-defined field, you must release the <c>UserFieldsMD</c> object using <c>System.Runtime.InteropServices.Marshal.ReleaseComObject()</c>.</description></item>
    /// <item><description>If you add a mandatory field, a default value must be provided; otherwise, an error will occur.</description></item>
    /// </list>
    /// <para>
    /// <b>Source table:</b> CUFD
    /// </para>
    /// </remarks>
    internal interface IUserTableField : IUserField, _Attribute {

        /// <summary>
        /// Gets or sets the technical name of the user-defined field (UDF).
        /// </summary>
        new string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the user-defined field.
        /// This is the human-readable label displayed in SAP Business One.
        /// </summary>
        new string Description { get; set; }

        /// <summary>
        /// Gets or sets a raw string array of valid values for the field.
        /// Used for fields with predefined value sets.
        /// </summary>
        string[] ValidValuePairs { get; set; }

        /// <summary>
        /// Gets the .NET type that corresponds to this user-defined field.
        /// </summary>
        Type Type { get; }

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

        string ToColumnData(object value);
    }
}