using System.Collections.Generic;

namespace SAPUtils.Query {
    /// <summary>
    /// Represents a single condition within a WHERE clause.
    /// This interface defines the properties that describe how a column is compared
    /// to a value, a range, a list of values, or the result of a subquery.
    /// </summary>
    public interface IWhereCondition {
        /// <summary>
        /// Gets the name of the column involved in the condition.
        /// </summary>
        string ColumnName { get; }

        /// <summary>
        /// Gets the type of SQL comparison to be performed (e.g., Equals, GreaterThan, Like).
        /// </summary>
        SqlComparison Comparison { get; }

        /// <summary>
        /// Gets the single value to which the column is being compared.
        /// This property is used for comparisons like Equals, GreaterThan, LessThan, etc.
        /// </summary>
        object ComparingValue { get; }

        /// <summary>
        /// Gets the range of values for a BETWEEN or NOT BETWEEN comparison.
        /// This is a nullable tuple containing the 'From' and 'To' values.
        /// </summary>
        (object From, object To)? ComparingBetween { get; }

        /// <summary>
        /// Gets the collection of values for an IN or NOT IN comparison.
        /// </summary>
        IEnumerable<object> ComparingIn { get; }

        /// <summary>
        /// Gets the subquery string for EXISTS or NOT EXISTS comparisons.
        /// </summary>
        string SubQuery { get; }
    }
}