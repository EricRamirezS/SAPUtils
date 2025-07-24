namespace SAPUtils.Query {
    /// <summary>
    /// Specifies the types of SQL comparison operators that can be used in a WHERE clause.
    /// These operators define how values or conditions are compared in a database query.
    /// </summary>
    public enum SqlComparison {
        /// <summary>
        /// Represents the equality operator (=).
        /// </summary>
        Equals,

        /// <summary>
        /// Represents the inequality operator (!= or &lt;&gt;).
        /// </summary>
        NotEquals,

        /// <summary>
        /// Represents the greater than operator (>).
        /// </summary>
        GreaterThan,

        /// <summary>
        /// Represents the less than operator (&lt;).
        /// </summary>
        LessThan,

        /// <summary>
        /// Represents the greater than or equal to operator (>=).
        /// </summary>
        GreaterThanOrEqual,

        /// <summary>
        /// Represents the less than or equal to operator (&lt;=).
        /// </summary>
        LessThanOrEqual,

        /// <summary>
        /// Represents the LIKE operator for pattern matching.
        /// </summary>
        Like,

        /// <summary>
        /// Represents the NOT LIKE operator for inverse pattern matching.
        /// </summary>
        NotLike,

        /// <summary>
        /// Represents the IN operator, checking if a value is within a set of values.
        /// </summary>
        In,

        /// <summary>
        /// Represents the NOT IN operator, checking if a value is not within a set of values.
        /// </summary>
        NotIn,

        /// <summary>
        /// Represents the BETWEEN operator, checking if a value falls within a specified range (inclusive).
        /// </summary>
        Between,

        /// <summary>
        /// Represents the NOT BETWEEN operator, checking if a value falls outside a specified range (inclusive).
        /// </summary>
        NotBetween,

        /// <summary>
        /// Represents the IS NULL operator, checking if a value is NULL.
        /// </summary>
        IsNull,

        /// <summary>
        /// Represents the IS NOT NULL operator, checking if a value is not NULL.
        /// </summary>
        IsNotNull,

        /// <summary>
        /// Represents the EXISTS operator, checking for the existence of any rows returned by a subquery.
        /// </summary>
        Exists,

        /// <summary>
        /// Represents the NOT EXISTS operator, checking for the non-existence of any rows returned by a subquery.
        /// </summary>
        NotExists,
    }
}