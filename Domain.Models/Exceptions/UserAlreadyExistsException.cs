using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions
{
    public sealed class UserAlreadyExistsException : Exception
    {
        public string Title { get; }
        public UserAlreadyExistsException(string email)
            : base($"A user with email '{email}' already exists.")
        {
            Title = "User Already Exists";
        }
    }
}
