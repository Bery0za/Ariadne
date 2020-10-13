using System;

namespace Bery0za.Ariadne
{
    public class BindingException : Exception
    {
        public BindingException(string message = null, Exception innerException = null)
            : base(message, innerException)
        {
            
        }
    }
}