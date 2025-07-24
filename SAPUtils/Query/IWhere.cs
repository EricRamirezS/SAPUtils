using System.Collections.Generic;

namespace SAPUtils.Query {
    /// <summary>
    /// Represents a grouping of conditions or subgroups used to build a SQL WHERE clause.
    /// This interface defines methods to retrieve the logical operator, conditions,
    /// and subgroups that form part of the group.
    /// </summary>
    /// <remarks>
    /// An implementation of this interface can be used to define complex logical groupings
    /// of conditions, with support for nested subgroups.
    /// </remarks>
    /// <seealso cref="IWhereCondition"/>
    /// <seealso cref="IWhereBuilder"/>
    /// <seealso cref="LogicalOperator"/>
    public interface IWhere {
        /// <summary>
        /// Gets the operator used to combine conditions or subgroups within a query group.
        /// </summary>
        /// <remarks>
        /// The <c>Operator</c> property specifies how conditions or subgroups within
        /// a query group are logically combined. The possible values are defined by
        /// the <see cref="SAPUtils.Query.LogicalOperator"/> enumeration, which includes
        /// logical operators such as <c>And</c> and <c>Or</c>.
        /// </remarks>
        /// <seealso cref="SAPUtils.Query.LogicalOperator"/>
        LogicalOperator Operator { get; }

        /// <summary>
        /// Gets a list of conditions within the current logical group.
        /// Each condition defines a comparison, value, or range that contributes to filtering or querying data.
        /// </summary>
        /// <remarks>
        /// A condition represents an individual constraint or a comparison statement, such as column-value comparisons,
        /// ranges (e.g., between two values), or inclusion in a set of values.
        /// </remarks>
        /// <seealso cref="SAPUtils.Query.IWhereCondition"/>
        List<IWhereCondition> Conditions { get; }

        /// <summary>
        /// Gets or sets the collection of subgroups represented as a list of <see cref="IWhere"/> objects.
        /// </summary>
        /// <remarks>
        /// SubGroups allow hierarchical grouping of conditions within a query. Each subgroup can have its own
        /// set of conditions or further nested subgroups, enabling complex logical structures in query generation.
        /// </remarks>
        /// <value>
        /// A list of <see cref="IWhere"/> instances representing the subgroups.
        /// </value>
        /// <seealso cref="LogicalOperator"/>
        /// <seealso cref="IWhereCondition"/>
        List<IWhere> SubGroups { get; }
    }
}