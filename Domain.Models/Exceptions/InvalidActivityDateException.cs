using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions
{
    public class InvalidActivityDateException : Exception
    {
        public InvalidActivityDateException(string message) : base(message)
        {
        }
    }
}
