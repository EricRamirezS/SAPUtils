using System;
using SAPbobsCOM;
using IUserTable = SAPUtils.__Internal.Attributes.UserTables.IUserTable;

// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to define a SAP Business One user-defined table (UDT).
    /// <br/>
    /// This attribute marks a class as a user table and specifies its name, description, 
    /// and primary key strategy.
    /// <br/>
    /// <b>Usage:</b>
    /// - Apply this attribute to a class to define it as a user table in SAP Business One.<br/>
    /// - The table's primary key strategy can be set using <see cref="PrimaryKeyStrategy"/>.<br/>
    /// <br/>
    /// <b>Example:</b>
    /// <code>
    /// [UserTable("ORDERS", "Customer Orders", PrimaryKeyStrategy.Serie)]
    /// public class OrderTable
    /// {
    ///     // Define table fields here
    /// }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class UserTableAttribute : Attribute, IUserTable {

        /// <inheritdoc />
        public UserTableAttribute(string name, string description) {
            Name = name;
            Description = description;
        }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Description { get; }

        /// <inheritdoc />
        public PrimaryKeyStrategy PrimaryKeyStrategy { get; set; } = PrimaryKeyStrategy.Serie;


        /// <inheritdoc />
        public BoUTBTableType TableType { get; set; } = BoUTBTableType.bott_NoObject;
    }
}