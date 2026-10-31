using AutoMapper;
using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.Module;
using LMS.Shared.Paging;
using Microsoft.AspNetCore.Identity;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Services
{
    public class ModuleService : IModuleService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ICourseService _courseService;
        public ModuleService(IUnitOfWork uow, IMapper mapper, ICourseService courseService)
        {
            _uow = uow;
            _mapper = mapper;
            _courseService = courseService;
        }

        public async Task<PagedResponse<ModuleDto>?> GetAllAsync(QueryParameters parameters, bool trackChanges = false)
        {
            var modules = await _uow.ModuleRepository.GetAllAsync(parameters, trackChanges);
            return modules != null ? _mapper.Map<PagedResponse<ModuleDto>>(modules) : null;
        }

        public async Task<PagedResponse<ModuleDto>?> GetByCourseIdAsync(int courseId, bool trackChanges = false)
        {
            await CheckCourseExistsAsync(courseId);

            var modules = await _uow.ModuleRepository.GetByCourseIdAsync(courseId, trackChanges);
            
            return _mapper.Map<PagedResponse<ModuleDto>>(modules);
        }

        public async Task<ModuleDto?> GetByIdAsync(int id, bool trackChanges = false)
        {
            var module = await _uow.ModuleRepository.GetByIdAsync(id, trackChanges);

            if (module is null)
            {
                throw new NotFoundException($"Module with id {id} not found.");
            }

            return _mapper.Map<ModuleDto>(module);
        }

        public async Task<ModuleDto?> GetByIdWithCourseAsync(int moduleId, bool trackChanges = false)
        {
            var module = await _uow.ModuleRepository.GetByIdWithCourseAsync(moduleId, trackChanges);

            if (module is null)
            {
                throw new NotFoundException($"Module with id {moduleId} not found.");
            }

            return _mapper.Map<ModuleDto>(module);
        }

        public async Task<bool> HasOverlappingDatesAsync(int courseId, DateTime startDate, DateTime endDate, int? excludeModuleId = null)
        {
            await ValidateModuleDatesAsync(courseId, startDate, endDate);

            return await _uow.ModuleRepository.HasOverlappingDatesAsync(courseId, startDate, endDate, excludeModuleId);
        }
        public async Task<ModuleDto> CreateModuleAsync(CreateModuleDto createModuleDto)
        {
            // Check if the course exists and validate the module dates
            await ValidateModuleDatesAsync(createModuleDto.CourseId, createModuleDto.StartDate, createModuleDto.EndDate);

            // Check for overlapping dates
            var isOverlapping = await _uow.ModuleRepository.HasOverlappingDatesAsync(createModuleDto.CourseId, createModuleDto.StartDate, createModuleDto.EndDate);
            if (isOverlapping)
            {
                throw new ModuleOverlappingException($"The module dates overlap with an existing module in the course with id {createModuleDto.CourseId}.");
            }

            var moduleEntity = _mapper.Map<Module>(createModuleDto);
            _uow.ModuleRepository.Create(moduleEntity);

            await _uow.CompleteAsync();
            return _mapper.Map<ModuleDto>(moduleEntity);
        }
        public async Task<ModuleDto> UpdateModuleAsync(int moduleId, UpdateModuleDto updateModuleDto)
        {

            var moduleEntity = await _uow.ModuleRepository.GetByIdAsync(moduleId, true);


            if (moduleEntity is null)
            {
                throw new NotFoundException(
                    $"Module with id {moduleId} not found.");
            }

            await ValidateModuleDatesAsync(
                updateModuleDto.CourseId,
                updateModuleDto.StartDate,
                updateModuleDto.EndDate);

            var isOverlapping =
                await _uow.ModuleRepository.HasOverlappingDatesAsync(
                    updateModuleDto.CourseId,
                    updateModuleDto.StartDate,
                    updateModuleDto.EndDate,
                    moduleId);

            if (isOverlapping)
            {
                throw new ModuleOverlappingException(
                    $"The module dates overlap with an existing module " +
                    $"in the course with id {updateModuleDto.CourseId}.");
            }

            _mapper.Map(updateModuleDto, moduleEntity);

            await _uow.CompleteAsync();

            return _mapper.Map<ModuleDto>(moduleEntity);
        }
        public async Task DeleteModuleAsync(int moduleId)
        {
            var moduleEntity = await _uow.ModuleRepository.GetByIdAsync(moduleId, true);
            if (moduleEntity is null)
            {
                throw new NotFoundException($"Module with id {moduleId} not found.");
            }

            _uow.ModuleRepository.Delete(moduleEntity);
            await _uow.CompleteAsync();
        }

        private async Task CheckCourseExistsAsync(int courseId)
        {
            var course = await _courseService.GetCourseAsync(courseId);
            if (course is null)
            {
                throw new NotFoundException($"Course with id {courseId} not found.");
            }
        }

        private async Task ValidateModuleDatesAsync(int courseId, DateTime startDate, DateTime endDate)
        {
            if (startDate > endDate)
            {
                throw new InvalidModuleDateException(
                    "Start date must be before or equal to end date.");
            }

            var course = await _courseService.GetCourseAsync(courseId);

            if (course is null)
            {
                throw new NotFoundException(
                    $"Course with id {courseId} not found.");
            }

            if (startDate < course.StartDate ||
                endDate > course.EndDate)
            {
                throw new InvalidModuleDateException(
                    "Module dates must be within the course dates.");
            }
        }
    }
}
