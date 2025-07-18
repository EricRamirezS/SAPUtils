using System;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to mark a class property as a SAP Business One user-defined field (UDF) 
    /// specifically for storing rate values.
    /// <br/>
    /// This attribute ensures that the field is stored as `db_Float` with the subtype `st_Rate`, 
    /// applying the system's rate accuracy settings.
    /// <br/>
    /// <b>Usage:</b><br/>
    /// - Apply this attribute to properties in a user table object model to define a rate field.<br/>
    /// - The precision is managed based on the system's rate accuracy settings.<br/>
    /// - Implements <see cref="IUserTableField{T}"/> for type-safe parsing and default value handling.<br/>
    /// <br/>
    /// <b>Example:</b>
    /// <code>
    /// [RateUserTableField(Name = "ExchangeRate", Description = "Exchange rate for currency conversion", Mandatory = true)]
    /// public double? ExchangeRate { get; set; }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RateFieldAttribute : DoubleUserTableFieldAttribute {
        /// <inheritdoc />
        public override BoFieldTypes FieldType => BoFieldTypes.db_Float;

        /// <inheritdoc />
        public override BoFldSubTypes SubType => BoFldSubTypes.st_Rate;
    }
}