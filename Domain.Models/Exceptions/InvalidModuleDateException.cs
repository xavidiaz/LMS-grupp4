using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions
{
    public class InvalidModuleDateException : Exception
    {
        public InvalidModuleDateException(string message) : base(message)
        {
        }
    }
}
