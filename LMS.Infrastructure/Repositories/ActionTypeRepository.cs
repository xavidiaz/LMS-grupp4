using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Extensions;
using LMS.Shared.Paging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Infrastructure.Repositories
{
    public class ActionTypeRepository : RepositoryBase<ActionType>, IActionTypeRepository
    {
        private readonly ApplicationDbContext _context;
        public ActionTypeRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IPagedList<ActionType>> GetAllAsync(QueryParameters parameters, bool trackChanges = false)
        {
            IQueryable<ActionType> query = _context.ActionTypes;

            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            return await query.OrderBy(m => m.Name).ToPagedListAsync(parameters);
        }
        public async Task<ActionType?> GetByActivityIdAsync(int activityId, bool trackChanges = false)
        {
            var activity = await _context.Activities.FindAsync(activityId);
            return activity?.ActionType;
        }
    }
}
