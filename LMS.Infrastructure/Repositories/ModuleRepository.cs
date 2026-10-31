using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Extensions;
using LMS.Shared.Paging;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories
{
    public class ModuleRepository: RepositoryBase<Module>, IModuleRepository
    {
        private readonly ApplicationDbContext _context;
        public ModuleRepository(ApplicationDbContext context): base(context)
        {
            _context = context;
        }

        public async Task<IPagedList<Module>> GetAllAsync(QueryParameters parameters, bool trackChanges = false)
        {
            IQueryable<Module> query = _context.Modules;

            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            return await query.OrderBy(m => m.StartDate).ToPagedListAsync(parameters);
        }

        public async Task<IPagedList<Module>> GetByCourseIdAsync(int courseId, QueryParameters parameters, bool trackChanges = false)
        {
            IQueryable<Module> query = _context.Modules.Where(m => m.CourseId == courseId).OrderBy(m => m.StartDate);

            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            return await query.ToPagedListAsync(parameters);
        }

        public async Task<IPagedList<Module>> GetByCourseIdAsync(int courseId, bool trackChanges = false)
        {
            IQueryable<Module> query = _context.Modules.Where(m => m.CourseId == courseId).OrderBy(m => m.StartDate);
   
            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            return await query.ToPagedListAsync(new QueryParameters());
        }

        public async Task<Module?> GetByIdAsync(int id, bool trackChanges = false)
        {
            IQueryable<Module> query = _context.Modules;

            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<Module?> GetByIdWithCourseAsync(int moduleId, bool trackChanges = false)
        {
            IQueryable<Module> query = _context.Modules.Include(m => m.Course);

            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(m => m.Id == moduleId);
        }

        public async Task<bool> HasOverlappingDatesAsync(
            int courseId,
            DateTime startDate,         
            DateTime endDate,
            int? excludeModuleId = null)
        {
            return await _context.Modules.AnyAsync(m =>
                m.CourseId == courseId &&
                (excludeModuleId == null || m.Id != excludeModuleId) &&
                m.StartDate <= endDate &&
                m.EndDate >= startDate);
        }
    }
}

