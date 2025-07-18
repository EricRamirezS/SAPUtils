using System;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to mark a class property as a SAP Business One user-defined field (UDF) 
    /// specifically for storing percentage values.
    /// <br/>
    /// This attribute ensures that the field is stored as `db_Float` with the subtype `st_Percentage`, 
    /// allowing accurate representation of percentage-based values.
    /// <br/>
    /// <b>Usage:</b><br/>
    /// - Apply this attribute to properties in a user table object model to define a percentage field.<br/>
    /// - The default precision is based on the system's price accuracy.<br/>
    /// - Implements <see cref="IUserTableField{T}"/> for type-safe parsing and default value handling.<br/>
    /// <br/>
    /// <b>Example:</b>
    /// <code>
    /// [PercentageUserTableField(Name = "DiscountRate", Description = "Customer discount percentage", Required = true)]
    /// public double? DiscountRate { get; set; }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PercentageFieldAttribute : DoubleUserTableFieldAttribute {

        /// <inheritdoc />
        public override BoFieldTypes FieldType => BoFieldTypes.db_Float;

        /// <inheritdoc />
        public override BoFldSubTypes SubType => BoFldSubTypes.st_Percentage;
    }
}