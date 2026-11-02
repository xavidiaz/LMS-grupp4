using Domain.Contracts;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Services
{
    public class CourseAccessService : ICourseAccessService
    {
        private readonly IUnitOfWork _uow;

        public CourseAccessService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<bool> CanAccessCourseAsync(
            string userId, int courseId)
        {
            var user = await _uow.UserRepsoitory.GetUserByIdAsync(userId);

            return user != null && user.CourseId == courseId;
        }

        public async Task<bool> CanAccessModuleAsync(
            string userId, int moduleId)
        {
            var user = await _uow.UserRepsoitory.GetUserByIdAsync(userId);

            if (user == null)
                return false;

            var module = await _uow.ModuleRepository.GetByIdAsync(moduleId);

            return module != null && module.CourseId == user.CourseId;
        }

        public async Task<bool> CanAccessActivityAsync(string userId, int activityId)
        {
            var user = await _uow.UserRepsoitory.GetUserByIdAsync(userId);

            if (user == null)
                return false;

            var activity = await _uow.ActivityRepository
                .GetByIdAsync(activityId);

            if (activity == null)
                return false;

            if (activity?.ModuleId is null)
                return false;

            var module = await _uow.ModuleRepository
                .GetByIdAsync(activity.ModuleId.Value);

            return module != null && module.CourseId == user.CourseId;
        }
    }
}
