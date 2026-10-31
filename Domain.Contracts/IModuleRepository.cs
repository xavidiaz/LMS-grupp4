using Domain.Models.Entities;
using LMS.Shared.Paging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts
{
    public interface IModuleRepository: IRepositoryBase<Module>
    {
        Task<Module?> GetByIdAsync(int id, bool trackChanges = false);
        Task<IPagedList<Module>> GetAllAsync(QueryParameters parameters, bool trackChanges = false);
        Task<IPagedList<Module>> GetByCourseIdAsync(int courseId, bool trackChanges = false);
        Task<Module?> GetByIdWithCourseAsync(int moduleId, bool trackChanges = false);
        Task<bool> HasOverlappingDatesAsync(int courseId,DateTime startDate, DateTime endDate,  int? excludeModuleId = null);
    }
}
