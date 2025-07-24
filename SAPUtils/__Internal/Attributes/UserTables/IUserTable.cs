using System.Diagnostics.CodeAnalysis;
using SAPbobsCOM;
using SAPUtils.Attributes.UserTables;

namespace SAPUtils.__Internal.Attributes.UserTables {
    /// <summary>
    /// Defines the contract for a SAP Business One user-defined table.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal interface IUserTable {
        /// <summary>
        /// Gets the name of the user-defined table in SAP Business One.
        /// </summary>
        /// <remarks>
        /// <b>Length:</b> 21 characters.
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// Gets the description of the user-defined table.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the primary key strategy for the table column "Code".
        /// </summary>
        PrimaryKeyStrategy PrimaryKeyStrategy { get; }

        /// <summary>
        /// Gets or sets the type of the user-defined table in SAP Business One.
        /// </summary>
        /// <remarks>
        /// This property corresponds to the <see cref="SAPbobsCOM.BoUTBTableType"/> in SAP Business One,
        /// and defines whether the table is a regular user table, a document table, or a master data table.
        /// </remarks>
        /// <value>
        /// A value of type <see cref="SAPbobsCOM.BoUTBTableType"/> that specifies the table type.
        /// </value>
        /// <seealso cref="SAPbobsCOM.BoUTBTableType"/>
        BoUTBTableType TableType { get; set; }
    }
}