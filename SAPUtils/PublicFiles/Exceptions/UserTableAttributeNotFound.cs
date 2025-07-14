using System;

namespace SAPUtils.Exceptions
{
    public class UserTableAttributeNotFound : Exception
    {
        public UserTableAttributeNotFound(string name) : base($"{name} is not decorated with UserTableAttribute")
        {
        }
    }
}