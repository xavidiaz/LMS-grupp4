using Domain.Models.Entities;
using Domain.Models.ReadModels;
using LMS.Shared.Paging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts
{
    public interface IActivityRepository : IRepositoryBase<Activity>
    {
        Task<Activity?> GetByIdAsync(int id, bool trackChanges = false);
        Task<IPagedList<Activity>> GetAllAsync(QueryParameters parameters, bool trackChanges = false);
        Task<IPagedList<Activity>> GetByModuleIdAsync(int moduleId, QueryParameters parameters, bool trackChanges = false);
        Task<Activity?> GetByIdWithModuleAsync(int activityId, bool trackChanges = false);
        Task<bool> HasOverlappingDatesAsync(int moduleId, DateTime startDate, DateTime endDate, int? excludeActivityId = null);
        Task<bool> IsWithinModulePeriodAsync(int moduleId, DateTime startDate, DateTime endDate);
        Task<IPagedList<StudentAssignmentReadModel>> GetStudentAssignmentsAsync(string userId, QueryParameters queryParameters, bool trackChanges = false);
    }
}
