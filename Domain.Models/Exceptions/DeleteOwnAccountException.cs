using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Exceptions
{
    public sealed class DeleteOwnAccountException : Exception
    {
        public string Title { get; } = "Cannot delete your own account";
        public DeleteOwnAccountException(string message)
            : base(message)
        {

        }
    }
}
