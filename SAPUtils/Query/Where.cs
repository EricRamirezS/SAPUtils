using SAPUtils.__Internal.Query;

namespace SAPUtils.Query {
    /// <summary>
    /// Provides a static factory method to create instances of <see cref="IWhereBuilder"/>.
    /// This class serves as the entry point for fluently constructing WHERE clauses.
    /// </summary>
    public static class Where {
        /// <summary>
        /// Creates a new instance of <see cref="IWhereBuilder"/> to begin constructing a WHERE clause.
        /// </summary>
        /// <returns>A new <see cref="IWhereBuilder"/> instance.</returns>
        public static IWhereBuilder Builder() {
            return new WhereBuilder();
        }
    }
}