using System;

namespace SAPUtils.Models.UserTables {
    /// <summary>
    /// Defines an interface for user-defined tables that require audit fields.  
    /// <br/>
    /// Implementing this interface ensures that audit-related fields are automatically  
    /// managed by the system, without requiring manual input.  
    /// <br/>
    /// <b>Note:</b>  
    /// - Classes implementing this interface must extend <see cref="SAPUtils.Models.UserTables.UserTableObjectModel{T}"/>.  
    /// - Properties decorated with <see cref="SAPUtils.Internal.Attributes.UserTables.IUserTableField"/>  
    ///   will be ignored for these fields.  
    /// - All values in this interface are set automatically by the system.  
    /// </summary>
    public interface IAuditableDate {
        /// <summary>
        /// The timestamp when the record was created.  
        /// <br/>
        /// <b>Automatically set when the record is inserted.</b>  
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// The timestamp when the record was last updated.  
        /// <br/>
        /// <b>Automatically updated on modification.</b>  
        /// </summary>
        DateTime UpdatedAt { get; set; }
    }
}