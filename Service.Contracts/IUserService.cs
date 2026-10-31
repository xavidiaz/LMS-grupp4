using LMS.Shared.DTOs.UserDtos;
using LMS.Shared.Paging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Contracts
{
    public interface IUserService
    {
        Task<PagedResponse<UserDto>> GetUsersAsync(QueryParameters query, bool trackChanges = false);
        Task<UserDto> GetUserAsync(string id, bool includeCourse = false, bool trackChanges = false);
        Task<UserDto> UpdateUserAsync(string id, UpdateUserDto dto);
        Task<CreateUserResponseDto> CreateUserAsync(CreateUserDto dto);
        Task DeleteUserAsync(string id, string currentUserId);
        Task SetPasswordAsync(SetPasswordDto dto);

        Task<IEnumerable<CourseParticipantDto>> GetCourseParticipantsAsync(int courseId);
    }
}
