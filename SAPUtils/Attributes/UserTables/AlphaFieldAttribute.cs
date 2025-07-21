using System;
using SAPUtils.__Internal.Attributes.UserTables;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to mark a class property as a SAP Business One user-defined field (UDF) 
    /// for storing alphanumeric data.
    /// <br/>
    /// This attribute is designed for fields that store string values in SAP Business One, ensuring the 
    /// correct field type (`db_Alpha`) and default subtype (`st_None`).
    /// <br/>
    /// <b>Usage:</b><br/>
    /// - Apply this attribute to properties in a user table object model to define an alphanumeric field.<br/>
    /// - The default maximum size is <b>50 characters</b>.<br/>
    /// - Implements <see cref="IUserTableField{T}"/> for type-safe parsing and default value handling.<br/>
    /// <br/>
    /// <b>Example:</b>
    /// <code>
    /// [AlphaUserTableField(Name = "CustomerCode", Description = "Unique Customer Identifier", Mandatory = true)]
    /// public string CustomerCode { get; set; }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class AlphaFieldAttribute : StringFieldAttribute { }
}