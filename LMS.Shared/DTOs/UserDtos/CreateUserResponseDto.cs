using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.UserDtos
{
    public sealed class CreateUserResponseDto
    {
        public UserDto User { get; set; } = null!;
        public string PasswordSetupToken { get; set; } = string.Empty;
    }
}
