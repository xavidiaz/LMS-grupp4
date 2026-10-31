using LMS.Shared.DTOs.ActionType;
using LMS.Shared.DTOs.Activity;
using LMS.Shared.DTOs.Module;
using LMS.Shared.Paging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Service.Contracts
{
    public interface IActionTypeService
    {
        Task<PagedResponse<ActionTypeDto>?> GetAllAsync(QueryParameters parameters, bool trackChanges = false);
        Task<ActionTypeDto?> GetByActivityIdAsync(int activityId, bool trackChanges = false);
    }
}
