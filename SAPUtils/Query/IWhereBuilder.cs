using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SAPUtils.Query {
    /// <summary>
    /// Represents a builder for constructing WHERE clauses for database queries.
    /// This interface provides a fluent API to define various conditions,
    /// including equality, comparisons, LIKE operations, NULL checks,
    /// BETWEEN, IN, and EXISTS clauses, as well as grouping conditions with logical operators.
    /// </summary>
    /// <example>
    /// This example demonstrates how to use the <see cref="IWhereBuilder"/> to construct a complex WHERE clause.
    /// <code>
    /// IWhere where = Where.Builder()
    ///     .Equals("Status", "Active")
    ///     .GreaterThan("Score", 75)
    ///     .Group(LogicalOperator.Or, group => group
    ///         .LessThan("Age", 18)
    ///         .GreaterThan("Age", 65)
    ///     )
    ///     .Build();
    /// </code>
    /// </example>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public interface IWhereBuilder {
        /// <summary>
        /// Groups a set of conditions together using the specified logical operator.
        /// </summary>
        /// <param name="op">The <see cref="LogicalOperator"/> to use for grouping (e.g., AND, OR).</param>
        /// <param name="groupAction">An action that takes an <see cref="IWhereBuilder"/> to define the conditions within the group.</param>
        /// <returns>The current <see cref="IWhereBuilder"/> instance for fluent chaining.</returns>
        IWhereBuilder Group(LogicalOperator op, Action<IWhereBuilder> groupAction);

        /// <summary>
        /// Adds an equality condition (column = value) to the WHERE clause.
        /// </summary>
        /// <param name="column">The name of the column.</param>
        /// <param name="value">The value to compare against.</param>
        /// <param name="isSystemColumn">If true, the column name is exactly as in database, otherwise <c>U_</c> prefix will be added to column name.</param>
        /// <returns>The current <see cref="IWhereBuilder"/> instance for fluent chaining.</returns>
        IWhereBuilder Equals(string column, object value, bool isSystemColumn = false);

        /// <summary>
        /// Adds a not-equals condition (column != value) to the WHERE clause.
        /// </summary>
        /// <param name="column">The name of the column.</param>
        /// <param name="value">The value to compare against.</param>
        /// <param name="isSystemColumn">If true, the column name is exactly as in database, otherwise <c>U_</c> prefix will be added to column name.</param>
        /// <returns>The current <see cref="IWhereBuilder"/> instance for fluent chaining.</returns>
        IWhereBuilder NotEquals(string column, object value, bool isSystemColumn = false);

        /// <summary>
        /// Adds a greater than condition (column > value) to the WHERE clause.
        /// </summary>
        /// <param name="column">The name of the column.</param>
        /// <param name="value">The value to compare against.</param>
        /// <param name="isSystemColumn">If true, the column name is exactly as in database, otherwise <c>U_</c> prefix will be added to column name.</param>
        /// <returns>The current <see cref="IWhereBuilder"/> instance for fluent chaining.</returns>
        IWhereBuilder GreaterThan(string column, object value, bool isSystemColumn = false);

        /// <summary>
        /// Adds a greater than or equal to condition (column >= value) to the WHERE clause.
        /// </summary>
        /// <param name="column">The name of the column.</param>
        /// <param name="value">The value to compare against.</param>
        /// <param name="isSystemColumn">If true, the column name is exactly as in database, otherwise <c>U_</c> prefix will be added to column name.</param>
        /// <returns>The current <see cref="IWhereBuilder"/> instance for fluent chaining.</returns>
        IWhereBuilder GreaterThanOrEqual(string column, object value, bool isSystemColumn = false);

        /// <summary>
        /// Adds a less than condition (column &lt; value) to the WHERE clause.
        /// </summary>
        /// <param name="column">The name of the column.</param>
        /// <param name="value">The value to compare against.</param>
        /// <param name="isSystemColumn">If true, the column name is exactly as in database, otherwise <c>U_</c> prefix will be added to column name.</param>
        /// <returns>The current <see cref="IWhereBuilder"/> instance for fluent chaining.</returns>
        IWhereBuilder LessThan(string column, object value, bool isSystemColumn = false);

        /// <summary>
        /// Adds a less than or equal to condition (column &lt;= value) to the WHERE clause.
        /// </summary>
        /// <param name="column">The name of the column.</param>
        /// <param name="value">The value to compare against.</param>
        /// <param name="isSystemColumn">If true, the column name is exactly as in database, otherwise <c>U_</c> prefix will be added to column name.</param>
        /// <returns>The current <see cref="IWhereBuilder"/> instance for fluent chaining.</returns>
        IWhereBuilder LessThanOrEqual(string column, object value, bool isSystemColumn = false);

        /// <summary>
        /// Adds a LIKE condition (column LIKE 'pattern') to the WHERE clause.
        /// </summary>
        /// <param name="column">The name of the column.</param>
        /// <param name="pattern">The pattern to match against (e.g., "%value%").</param>
        /// <param name="isSystemColumn">If true, the column name is exactly as in database, otherwise <c>U_</c> prefix will be added to column name.</param>
        /// <returns>The current <see cref="IWhereBuilder"/> instance for fluent chaining.</returns>
        IWhereBuilder Like(string column, string pattern, bool isSystemColumn = false);

        /// <summary>
        /// Adds a NOT LIKE condition (column NOT LIKE 'pattern') to the WHERE clause.
        /// </summary>
        /// <param name="column">The name of the column.</param>
        /// <param name="pattern">The pattern to not match against (e.g., "%value%").</param>
        /// <param name="isSystemColumn">If true, the column name is exactly as in database, otherwise <c>U_</c> prefix will be added to column name.</param>
        /// <returns>The current <see cref="IWhereBuilder"/> instance for fluent chaining.</returns>
        IWhereBuilder NotLike(string column, string pattern, bool isSystemColumn = false);

        /// <summary>
        /// Adds an IS NULL condition (column IS NULL) to the WHERE clause.
        /// </summary>
        /// <param name="column">The name of the column.</param>
        /// <param name="isSystemColumn">If true, the column name is exactly as in database, otherwise <c>U_</c> prefix will be added to column name.</param>
        /// <returns>The current <see cref="IWhereBuilder"/> instance for fluent chaining.</returns>
        IWhereBuilder IsNull(string column, bool isSystemColumn = false);

        /// <summary>
        /// Adds an IS NOT NULL condition (column IS NOT NULL) to the WHERE clause.
        /// </summary>
        /// <param name="column">The name of the column.</param>
        /// <param name="isSystemColumn">If true, the column name is exactly as in database, otherwise <c>U_</c> prefix will be added to column name.</param>
        /// <returns>The current <see cref="IWhereBuilder"/> instance for fluent chaining.</returns>
        IWhereBuilder IsNotNull(string column, bool isSystemColumn = false);

        /// <summary>
        /// Adds a BETWEEN condition (column BETWEEN from AND to) to the WHERE clause.
        /// </summary>
        /// <param name="column">The name of the column.</param>
        /// <param name="from">The starting value of the range.</param>
        /// <param name="to">The ending value of the range.</param>
        /// <param name="isSystemColumn">If true, the column name is exactly as in database, otherwise <c>U_</c> prefix will be added to column name.</param>
        /// <returns>The current <see cref="IWhereBuilder"/> instance for fluent chaining.</returns>
        IWhereBuilder Between(string column, object from, object to, bool isSystemColumn = false);

        /// <summary>
        /// Adds a NOT BETWEEN condition (column NOT BETWEEN from AND to) to the WHERE clause.
        /// </summary>
        /// <param name="column">The name of the column.</param>
        /// <param name="from">The starting value of the range.</param>
        /// <param name="to">The ending value of the range.</param>
        /// <param name="isSystemColumn">If true, the column name is exactly as in database, otherwise <c>U_</c> prefix will be added to column name.</param>
        /// <returns>The current <see cref="IWhereBuilder"/> instance for fluent chaining.</returns>
        IWhereBuilder NotBetween(string column, object from, object to, bool isSystemColumn = false);

        /// <summary>
        /// Adds an IN condition (column IN (values)) to the WHERE clause.
        /// </summary>
        /// <param name="column">The name of the column.</param>
        /// <param name="values">A collection of values to check against.</param>
        /// <param name="isSystemColumn">If true, the column name is exactly as in database, otherwise <c>U_</c> prefix will be added to column name.</param>
        /// <returns>The current <see cref="IWhereBuilder"/> instance for fluent chaining.</returns>
        IWhereBuilder In<T>(string column, IEnumerable<T> values, bool isSystemColumn = false);

        /// <summary>
        /// Adds a NOT IN condition (column NOT IN (values)) to the WHERE clause.
        /// </summary>
        /// <param name="column">The name of the column.</param>
        /// <param name="values">A collection of values to not check against.</param>
        /// <param name="isSystemColumn">If true, the column name is exactly as in database, otherwise <c>U_</c> prefix will be added to column name.</param>
        /// <returns>The current <see cref="IWhereBuilder"/> instance for fluent chaining.</returns>
        IWhereBuilder NotIn<T>(string column, IEnumerable<T> values, bool isSystemColumn = false);

        /// <summary>
        /// Adds an EXISTS condition (EXISTS (subquery)) to the WHERE clause.
        /// </summary>
        /// <param name="subquery">The subquery to check for existence.</param>
        /// <returns>The current <see cref="IWhereBuilder"/> instance for fluent chaining.</returns>
        IWhereBuilder Exists(string subquery);

        /// <summary>
        /// Adds a NOT EXISTS condition (NOT EXISTS (subquery)) to the WHERE clause.
        /// </summary>
        /// <param name="subquery">The subquery to check for non-existence.</param>
        /// <returns>The current <see cref="IWhereBuilder"/> instance for fluent chaining.</returns>
        IWhereBuilder NotExists(string subquery);

        /// <summary>
        /// Builds and returns the constructed WHERE clause.
        /// </summary>
        /// <returns>An <see cref="IWhere"/> object representing the completed WHERE clause.</returns>
        IWhere Build();
    }
}