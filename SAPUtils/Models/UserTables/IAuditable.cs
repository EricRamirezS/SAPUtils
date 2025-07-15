using SAPUtils.__Internal.Attributes.UserTables;

namespace SAPUtils.Models.UserTables {
    /// <summary>
    /// Defines an interface for user-defined tables that require audit fields.  
    /// <br/>
    /// Implementing this interface ensures that audit-related fields are automatically  
    /// managed by the system, without requiring manual input.  
    /// <br/>
    /// <b>Note:</b>  
    /// - Classes implementing this interface must extend <see cref="SAPUtils.Models.UserTables.UserTableObjectModel{T}"/>.  
    /// - Properties decorated with <see cref="IUserTableField"/>  
    ///   will be ignored for these fields.  
    /// - All values in this interface are set automatically by the system.  
    /// </summary>
    public interface IAuditable : IAuditableDate, IAuditableUser, ISoftDeletable { }
}