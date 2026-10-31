using Domain.Models.Entities;
using LMS.Shared.Paging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts
{
    public interface IActionTypeRepository : IRepositoryBase<ActionType>
    {
        Task<IPagedList<ActionType>> GetAllAsync(QueryParameters parameters, bool trackChanges = false);
        Task<ActionType?> GetByActivityIdAsync(int activityId, bool trackChanges = false);
    }
}
