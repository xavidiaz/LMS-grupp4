using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions
{
    public class CourseInvalidDatesException : Exception
    {
        public string Title { get; } = "Invalid data";

        public CourseInvalidDatesException(string message) : base(message)
        {
        }
    }
}
