// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMethodReturnValue.Global

namespace SAPUtils.Models.UserTables {
    /// <summary>
    /// Defines the base contract for user-defined table objects in SAP.  
    /// <br/>
    /// Implementing classes should manage their own persistence within SAP B1.  
    /// </summary>
    public interface IUserTableObjectModel {
        /// <summary>
        /// The unique code identifier for the record.  
        /// </summary>
        string Code { get; set; }

        /// <summary>
        /// The name or description of the record.  
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Adds a new record to the SAP user table.  
        /// </summary>
        /// <returns>True if the operation is successful, otherwise false.</returns>
        bool Add();

        /// <summary>
        /// Deletes the current record from the SAP user table.  
        /// </summary>
        /// <returns>True if the operation is successful, otherwise false.</returns>
        bool Delete();

        /// <summary>
        /// Updates an existing record in the SAP user table.  
        /// </summary>
        /// <returns>True if the operation is successful, otherwise false.</returns>
        bool Update(bool restore = false);

        /// <summary>
        /// Saves the record, performing either an insert or an update depending on  
        /// whether the record already exists.  
        /// </summary>
        /// <returns>True if the operation is successful, otherwise false.</returns>
        bool Save();
    }
}