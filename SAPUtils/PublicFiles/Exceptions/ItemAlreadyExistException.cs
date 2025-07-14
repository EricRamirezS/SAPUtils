using System;

namespace SAPUtils.Exceptions
{
    public class ItemAlreadyExistException : Exception
    {
        public ItemAlreadyExistException(string name, string code) : base(
            $"Item with code {code} already exists in table {name}.")
        {
        }
    }
}