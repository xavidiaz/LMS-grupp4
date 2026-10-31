using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions
{
    public class CourseNameRequiredException : Exception
    {
        public string Title { get; } = "Invalid data";
        
        public CourseNameRequiredException(string message) : base(message)
        {
        }
    }
}
