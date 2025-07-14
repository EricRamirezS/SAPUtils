using System;

namespace SAPUtils.Exceptions
{
    public class CodeNotSetException : Exception
    {
        public CodeNotSetException(string tableName) : base(
            $"No Code was provided for new data in {tableName}, but PrimaryKeyStrategy was set to Manual")
        {
        }
    }
}