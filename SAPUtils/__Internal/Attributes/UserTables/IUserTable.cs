using SAPUtils.Attributes.UserTables;

namespace SAPUtils.__Internal.Attributes.UserTables {
    /// <summary>
    /// Defines the contract for a SAP Business One user-defined table.
    /// </summary>
    internal interface IUserTable {
        /// <summary>
        /// Gets the name of the user-defined table in SAP Business One.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the description of the user-defined table.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets the primary key strategy for the table column "Code".
        /// </summary>
        PrimaryKeyStrategy PrimaryKeyStrategy { get; }
    }
}