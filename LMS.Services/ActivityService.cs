using AutoMapper;
using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.Activity;
using LMS.Shared.DTOs.AssignmentDtos;
using LMS.Shared.Paging;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Services
{
    public class ActivityService : IActivityService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public ActivityService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<PagedResponse<ActivityDto>?> GetAllAsync(QueryParameters parameters, bool trackChanges = false)
        {
            var activities = await _uow.ActivityRepository.GetAllAsync(parameters, trackChanges);

            return _mapper.Map<PagedResponse<ActivityDto>>(activities);
        }


        public async Task<PagedResponse<ActivityDto>?> GetByModuleIdAsync(
            int moduleId,
            QueryParameters parameters,
            bool trackChanges = false)
        {
            await CheckModuleExistsAsync(moduleId);

            var activities = await _uow.ActivityRepository
                .GetByModuleIdAsync(moduleId, parameters, trackChanges);

            var activityDtos = _mapper.Map<List<ActivityDto>>(activities.Items);

            return new PagedResponse<ActivityDto>(
                activityDtos,
                parameters.PageNumber,
                parameters.PageSize,
                activities.TotalCount);
        }


        public async Task<ActivityDto?> GetByIdAsync(int id, bool trackChanges = false)
        {
            var activity = await _uow.ActivityRepository.GetByIdAsync(id, trackChanges);

            if (activity is null)
            {
                throw new NotFoundException(
                    $"Activity with id {id} not found.");
            }

            return _mapper.Map<ActivityDto>(activity);
        }

        public async Task<ActivityDto?> GetByIdWithModuleAsync(int activityId, bool trackChanges = false)
        {
            var activity = await _uow.ActivityRepository.GetByIdWithModuleAsync(activityId, trackChanges);

            if (activity is null)
            {
                throw new NotFoundException(
                    $"Activity with id {activityId} not found.");
            }

            return _mapper.Map<ActivityDto>(activity);
        }

        public async Task<ActivityDto> CreateActivityAsync(CreateActivityDto createActivityDto)
        {
            await ValidateActivityAsync(createActivityDto.ModuleId, createActivityDto.StartDate, createActivityDto.EndDate);

            var isOverlapping = await _uow.ActivityRepository.HasOverlappingDatesAsync(

                    createActivityDto.ModuleId,
                    createActivityDto.StartDate,
                    createActivityDto.EndDate);

            if (isOverlapping)
            {
                throw new ActivityOverlappingException(
                    $"The activity dates overlap with another activity " +
                    $"in module {createActivityDto.ModuleId}.");
            }

            var activityEntity = _mapper.Map<Activity>(createActivityDto);

            _uow.ActivityRepository.Create(activityEntity);

            await _uow.CompleteAsync();

            return _mapper.Map<ActivityDto>(activityEntity);
        }

        public async Task<ActivityDto> UpdateActivityAsync(
            int activityId,
            UpdateActivityDto updateActivityDto)
        {
            var activityEntity =
                await _uow.ActivityRepository.GetByIdAsync(
                    activityId, trackChanges: true);

            if (activityEntity is null)
            {
                throw new NotFoundException(
                    $"Activity with id {activityId} not found.");
            }

            await ValidateActivityAsync(
                updateActivityDto.ModuleId,
                updateActivityDto.StartDate,
                updateActivityDto.EndDate);

            var isOverlapping =
                await _uow.ActivityRepository.HasOverlappingDatesAsync(
                    updateActivityDto.ModuleId,
                    updateActivityDto.StartDate,
                    updateActivityDto.EndDate,
                    activityId);

            if (isOverlapping)
            {
                throw new ActivityOverlappingException(
                    $"The activity dates overlap with another activity " +
                    $"in module {updateActivityDto.ModuleId}.");
            }

            _mapper.Map(updateActivityDto, activityEntity);

            await _uow.CompleteAsync();

            return _mapper.Map<ActivityDto>(activityEntity);
        }

        public async Task DeleteActivityAsync(int activityId)
        {
            var activityEntity = await _uow.ActivityRepository.GetByIdAsync(activityId, trackChanges: true);

            if (activityEntity is null)
            {
                throw new NotFoundException(
                    $"Activity with id {activityId} not found.");
            }

            _uow.ActivityRepository.Delete(activityEntity);

            await _uow.CompleteAsync();
        }

        private async Task CheckModuleExistsAsync(int moduleId)
        {
            var module = await _uow.ModuleRepository.GetByIdAsync(moduleId);

            if (module is null)
            {
                throw new NotFoundException(
                    $"Module with id {moduleId} not found.");
            }
        }

        private async Task ValidateActivityAsync(int moduleId, DateTime startDate,DateTime endDate)
        {
            if (startDate >= endDate)
            {
                throw new InvalidActivityDateException(
                    "Activity start time must be before its end time.");
            }

            var module =
                await _uow.ModuleRepository.GetByIdAsync(moduleId);

            if (module is null)
            {
                throw new NotFoundException(
                    $"Module with id {moduleId} not found.");
            }

            if (startDate < module.StartDate ||
                endDate > module.EndDate)
            {
                throw new InvalidActivityDateException(
                    "Activity dates must be within the module's date range.");
            }
        }

        public async Task<PagedResponse<AssignmentDetailsDto>?> GetStudentAssignmentsAsync(
        string userId,
        QueryParameters parameters,
        bool trackChanges = false)
        {
            var assignments =
                await _uow.ActivityRepository.GetStudentAssignmentsAsync(
                    userId,
                    parameters,
                    trackChanges);

            return _mapper.Map<PagedResponse<AssignmentDetailsDto>>(
                assignments);
        }
    }
}
