using LMS.Shared.DTOs.Activity;
using LMS.Shared.DTOs.AssignmentDtos;
using LMS.Shared.Paging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Contracts
{
    public interface IActivityService
    {
        Task<PagedResponse<ActivityDto>?> GetAllAsync(QueryParameters parameters, bool trackChanges = false);
        Task<PagedResponse<ActivityDto>?> GetByModuleIdAsync(int moduleId, QueryParameters parameters, bool trackChanges = false);
        Task<ActivityDto?> GetByIdAsync(int id, bool trackChanges = false);
        Task<ActivityDto?> GetByIdWithModuleAsync(int activityId, bool trackChanges = false);
        Task<ActivityDto> CreateActivityAsync(CreateActivityDto createActivityDto);
        Task<ActivityDto> UpdateActivityAsync(int activityId, UpdateActivityDto updateActivityDto);
        Task DeleteActivityAsync(int activityId);
        Task<PagedResponse<AssignmentDetailsDto>?> GetStudentAssignmentsAsync(string userId, QueryParameters parameters, bool trackChanges = false);
    }
}
