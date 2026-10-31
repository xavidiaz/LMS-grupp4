using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.UserDtos
{
    public sealed class SetPasswordDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
