namespace SAPUtils.Attributes.UserTables
{
    /// <summary>
    /// Specifies the different primary key strategies available for user-defined tables.
    /// </summary>
    public enum PrimaryKeyStrategy
    {
        /// <summary>
        /// The primary key is a globally unique identifier (GUID).
        /// </summary>
        Guid,
        /// <summary>
        /// The primary key follows a sequential series pattern.
        /// </summary>
        Serie,
        /// <summary>
        /// The primary key is manually assigned.
        /// </summary>
        Manual,
    }
}