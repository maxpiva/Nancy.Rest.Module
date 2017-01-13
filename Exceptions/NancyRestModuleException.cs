using System;

namespace Nancy.Rest.Module.Exceptions
{
    public class NancyRestModuleException : Exception
    {
        public NancyRestModuleException(string message) : base(message)
        {
            
        }
    }
}
