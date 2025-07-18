using System;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace SAPUtils.Attributes.UserTables {

    /// <summary>
    /// Represents an attribute used to mark a class property as a SAP Business One user-defined field (UDF) 
    /// specifically for storing quantity values.
    /// <br/>
    /// This attribute ensures that the field is stored as `db_Float` with the subtype `st_Quantity`, 
    /// applying the system's quantity accuracy settings.
    /// <br/>
    /// <b>Usage:</b><br/>
    /// - Apply this attribute to properties in a user table object model to define a quantity field.<br/>
    /// - The precision is managed based on the system's quantity accuracy settings.<br/>
    /// - Implements <see cref="IUserTableField{T}"/> for type-safe parsing and default value handling.<br/>
    /// <br/>
    /// <b>Example:</b>
    /// <code>
    /// [QuantityUserTableField(Name = "StockQuantity", Description = "Quantity of stock available", Mandatory = true)]
    /// public double? StockQuantity { get; set; }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class QuantityFieldAttribute : DoubleUserTableFieldAttribute {
        /// <inheritdoc />
        public override BoFieldTypes FieldType => BoFieldTypes.db_Float;

        /// <inheritdoc />
        public override BoFldSubTypes SubType => BoFldSubTypes.st_Quantity;
    }
}