using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions
{
    public class InvalidSubmissionStatusException : Exception
    {
        public InvalidSubmissionStatusException(string message) : base(message)
        {
        }
    }
}
