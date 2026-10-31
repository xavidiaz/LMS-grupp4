using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions
{
    public class ModuleOverlappingException : Exception
    {
        public ModuleOverlappingException(string message) : base(message)
        {
        }
    }
}
