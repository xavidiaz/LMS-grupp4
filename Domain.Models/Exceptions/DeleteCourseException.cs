using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions
{
    public sealed class DeleteCourseException : Exception
    {
        public string Title { get; } = "Cannot delete course";

        public DeleteCourseException(string message)
            : base(message)
        {
        }

        public DeleteCourseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

}
