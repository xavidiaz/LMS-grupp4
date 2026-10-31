using Domain.Models.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.ReadModels
{
    public sealed class UserWithRole
    {
        public ApplicationUser User { get; init; } = null!;
        public string? Role { get; init; }
    }
}
