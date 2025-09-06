using System;
using SAPUtils.I18N;

namespace SAPUtils.Exceptions {
    /// <summary>
    /// Represents an exception that is thrown when an item with a specified code does not exist in a given table.
    /// </summary>
    public class ItemDoNotExistException : Exception {
        internal ItemDoNotExistException(string className, string name, string code) : base(
            string.Format(Texts.ItemDoNotExistException_ItemDoNotExistException__0__with_code__1__does_not_exist_in_table__2__, className, code, name)) { }
    }
}