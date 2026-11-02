using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Contracts
{
    public interface ICourseAccessService
    {
        Task<bool> CanAccessCourseAsync(string userId, int courseId);
        Task<bool> CanAccessModuleAsync(string userId, int moduleId);
        Task<bool> CanAccessActivityAsync(string userId, int activityId);
    }
}
