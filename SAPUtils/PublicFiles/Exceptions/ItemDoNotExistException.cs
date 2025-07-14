using System;

namespace SAPUtils.Exceptions {
    /// <summary>
    /// Represents an exception that is thrown when an item with a specified code does not exist in a given table.
    /// </summary>
    public class ItemDoNotExistException : Exception {
        internal ItemDoNotExistException(string className, string name, string code) : base(
            $"{className} with code {code} does not exist in table {name}.") { }
    }
}