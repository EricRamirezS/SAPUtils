using System;
using SAPUtils.I18N;

namespace SAPUtils.Exceptions {
    /// <summary>
    /// The <c>ItemAlreadyExistException</c> class is a specific exception type
    /// that represents the scenario where an item with a given code already exists
    /// in a specified table.
    /// </summary>
    /// <remarks>
    /// This exception is typically thrown during add operations to indicate that
    /// the insertion cannot proceed because an item with the same code is already present
    /// in the target table.
    /// </remarks>
    /// <seealso cref="System.Exception"/>
    /// <seealso cref="SAPUtils.Models.UserTables.UserTableObjectModel{T}"/>
    public class ItemAlreadyExistException : Exception {
        /// <summary>
        /// Represents an exception that occurs when an item with a specific code already exists in a table.
        /// </summary>
        /// <remarks>
        /// This exception is typically thrown when attempting to add an item with a duplicate key to a table
        /// using a manual primary key strategy.
        /// </remarks>
        /// <param name="name">The name of the table where the duplicate item is being inserted.</param>
        /// <param name="code">The duplicate code of the item that is already present in the table.</param>
        /// <seealso cref="SAPUtils.Models.UserTables.UserTableObjectModel{T}"/>
        public ItemAlreadyExistException(string name, string code) : base(
            string.Format(Texts.ItemAlreadyExistException_ItemAlreadyExistException_Item_with_code__0__already_exists_in_table__1__, code, name)) { }
    }
}