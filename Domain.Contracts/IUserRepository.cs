using Domain.Models.Entities;
using Domain.Models.ReadModels;
using LMS.Shared.Paging;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Domain.Contracts
{
    public interface IUserRepository : IRepositoryBase<ApplicationUser>
    {
        Task<ApplicationUser?> GetUserByEmailAsync(string email, bool includeCourse = false, bool trackChanges = false);
        Task<ApplicationUser?> GetUserByIdAsync(string id, bool includeCourse = false, bool trackChanges = false);
        Task<IPagedList<ApplicationUser>> GetAllUsersAsync(QueryParameters parameters, bool includeCourse = false, bool trackChanges = false);
        Task<IPagedList<UserWithRole>> GetAllUsersWithRolesAsync(QueryParameters parameters);

        Task<IEnumerable<ApplicationUser>> GetUsersByCourseIdAsync(int courseId, bool trackChanges = false);

    }
}
