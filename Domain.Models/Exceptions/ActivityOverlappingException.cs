using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions
{
    public class ActivityOverlappingException : Exception
    {
        public ActivityOverlappingException(string message) : base(message)
        {
        }
    }
}
