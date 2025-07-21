using System;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to mark a class property as a SAP Business One user-defined field (UDF) 
    /// specifically for storing address-related data.
    /// <br/>
    /// This attribute is designed for fields that store addresses within SAP Business One, ensuring the 
    /// correct field type (`db_Alpha`) and subtype (`st_Address`).
    /// <br/>
    /// <b>Usage:</b><br/>
    /// - Apply this attribute to properties in a user table object model to define an address field.<br/>
    /// - The default maximum size is **254 characters**.<br/>
    /// - Implements <see cref="IUserTableField{T}"/> for type-safe parsing and default value handling.<br/>
    /// <br/>
    /// <b>Example:</b>
    /// <code>
    /// [AddressUserTableField(Name = "BillingAddress", Description = "Customer Billing Address", Mandatory = true)]
    /// public string BillingAddress { get; set; }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class AddressFieldAttribute : StringFieldAttribute {
        /// <inheritdoc />
        public override BoFldSubTypes SubType => BoFldSubTypes.st_Address;
    }
}