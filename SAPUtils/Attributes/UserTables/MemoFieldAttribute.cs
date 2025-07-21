using System;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to mark a class property as a SAP Business One user-defined field (UDF) 
    /// specifically for storing large text data.
    /// <br/>
    /// This attribute ensures that the field is stored as a `db_Memo`, allowing for long text storage without 
    /// a predefined length limit.
    /// <br/>
    /// <b>Usage:</b><br/>
    /// - Apply this attribute to properties in a user table object model to define a memo (large text) field.<br/>
    /// - Implements <see cref="IUserTableField{T}"/> for type-safe parsing and default value handling.<br/>
    /// - The default maximum size is set to <see cref="int.MaxValue"/>.<br/>
    /// <br/>
    /// <b>Example:</b>
    /// <code>
    /// [MemoUserTableField(Name = "Comments", Description = "Detailed customer feedback")]
    /// public string Comments { get; set; }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MemoFieldAttribute : StringFieldAttribute {
        /// <inheritdoc />
        public override BoFieldTypes FieldType => BoFieldTypes.db_Memo;

        /// <inheritdoc />
        public override int Size { get; set; } = int.MaxValue;
    }
}