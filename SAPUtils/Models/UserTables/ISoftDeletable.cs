namespace SAPUtils.Models.UserTables {
    /// <summary>
    /// Defines an interface for entities that support soft deletion.
    /// <br/>
    /// Implementing this interface allows the system to mark records as inactive 
    /// instead of physically deleting them from the database.
    /// <br/>
    /// <b>Note:</b>  
    /// - This field is intended for audit purposes.  
    /// - Classes implementing this interface should treat <c>Active = false</c> as deleted.  
    /// - The value is automatically managed by the system and should not be set manually.  
    /// </summary>
    public interface ISoftDeletable {
        /// <summary>
        /// Indicates whether the record is active.  
        /// <br/>
        /// <b>Automatically set by the system.</b>  
        /// </summary>
        bool Active { get; set; }
    }
}