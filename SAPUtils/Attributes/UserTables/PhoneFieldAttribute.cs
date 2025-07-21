using System;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to mark a class property as a SAP Business One user-defined field (UDF) 
    /// specifically for storing phone numbers.
    /// <br/>
    /// This attribute ensures that the field is stored as `db_Alpha` with the subtype `st_Phone`, 
    /// providing appropriate formatting for phone numbers.
    /// <br/>
    /// <b>Usage:</b><br/>
    /// - Apply this attribute to properties in a user table object model to define a phone number field.<br/>
    /// - Implements <see cref="IUserTableField{T}"/> for type-safe parsing and default value handling.<br/>
    /// <br/>
    /// <b>Example:</b>
    /// <code>
    /// [PhoneUserTableField(Name = "CustomerPhone", Description = "Primary contact phone number", Mandatory = true)]
    /// public string CustomerPhone { get; set; }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PhoneFieldAttribute : StringFieldAttribute {
        /// <inheritdoc />
        public override BoFldSubTypes SubType => BoFldSubTypes.st_Phone;

        /// <inheritdoc />
        public override int Size { get; set; } = 20;
    }
}