using System.Diagnostics.CodeAnalysis;

namespace SAPUtils.Query {
    /// <summary>
    /// Specifies the logical operators used to combine conditions in a WHERE clause.
    /// These operators determine how multiple conditions are evaluated together.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum LogicalOperator {
        /// <summary>
        /// Represents the logical AND operator. Conditions combined with AND must all be true for the overall condition to be true.
        /// </summary>
        And,

        /// <summary>
        /// Represents the logical OR operator. At least one of the conditions combined with OR must be true for the overall condition to be true.
        /// </summary>
        Or,
    }
}