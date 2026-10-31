using AutoMapper;
using Domain.Contracts;
using LMS.Shared.DTOs.ActionType;
using LMS.Shared.DTOs.Activity;
using LMS.Shared.Paging;
using Service.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Services
{
    public class ActionTypeService : IActionTypeService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public ActionTypeService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<PagedResponse<ActionTypeDto>?> GetAllAsync(QueryParameters parameters, bool trackChanges = false)
        {
            var actionTypes = await _uow.ActionTypeRepository.GetAllAsync(parameters, trackChanges);

            return _mapper.Map<PagedResponse<ActionTypeDto>>(actionTypes);
        }
        public async Task<ActionTypeDto?> GetByActivityIdAsync(int activityId, bool trackChanges = false)
        {
            var actionType = await _uow.ActionTypeRepository.GetByActivityIdAsync(activityId, trackChanges);
            return _mapper.Map<ActionTypeDto>(actionType);
        }
    }
}
