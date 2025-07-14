using System;

namespace SAPUtils.Exceptions
{
    public class AddontNotInitializedException : Exception
    {
        public AddontNotInitializedException() : base(
            "El add-on no ha sido inicializado, ") { }
    }
}