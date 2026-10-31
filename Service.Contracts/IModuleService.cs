using LMS.Shared.DTOs.Module;
using LMS.Shared.Paging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Service.Contracts
{
    public interface IModuleService
    {
        Task<ModuleDto?> GetByIdAsync(int id, bool trackChanges = false);
        Task<PagedResponse<ModuleDto>?> GetAllAsync(QueryParameters parameters, bool trackChanges = false);
        Task<PagedResponse<ModuleDto>?> GetByCourseIdAsync(int courseId, bool trackChanges = false);
        Task<ModuleDto?> GetByIdWithCourseAsync(int moduleId, bool trackChanges = false);
        Task<bool> HasOverlappingDatesAsync(int courseId, DateTime startDate, DateTime endDate, int? excludeModuleId = null);
        Task<ModuleDto> UpdateModuleAsync(int moduleId, UpdateModuleDto updateModuleDto);
        Task<ModuleDto> CreateModuleAsync(CreateModuleDto createModuleDto);
        Task DeleteModuleAsync(int moduleId);
    }
}
