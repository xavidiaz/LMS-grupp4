using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.ReadModels;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Extensions;
using LMS.Shared.Paging;
using Microsoft.EntityFrameworkCore;


namespace LMS.Infrastructure.Repositories
{
    public class ActivityRepository : RepositoryBase<Activity>, IActivityRepository
    {
        private readonly ApplicationDbContext _context;

        public ActivityRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<IPagedList<Activity>> GetAllAsync(
            QueryParameters parameters,
            bool trackChanges = false)
        {
            IQueryable<Activity> query = _context.Activities
                .Include(a => a.ActionType)
                .OrderBy(a => a.StartDate);

            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            return await query.ToPagedListAsync(parameters);
        }

        // Get activities belonging to a specific module
        public async Task<IPagedList<Activity>> GetByModuleIdAsync(
            int moduleId,
            QueryParameters parameters,
            bool trackChanges = false)
        {
            IQueryable<Activity> query = _context.Activities
                .Where(a => a.ModuleId == moduleId)
                .Include(a => a.ActionType)
                .OrderBy(a => a.StartDate);

            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            return await query.ToPagedListAsync(parameters);
        }

        public async Task<Activity?> GetByIdAsync(
            int id,
            bool trackChanges = false)
        {
            IQueryable<Activity> query = _context.Activities;

            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Activity?> GetByIdWithModuleAsync(
            int activityId,
            bool trackChanges = false)
        {
            IQueryable<Activity> query = _context.Activities
                .Include(a => a.Module)
                .Include(a => a.ActionType);

            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(
                a => a.Id == activityId);
        }

        public async Task<bool> HasOverlappingDatesAsync(
            int moduleId,
            DateTime startDate,
            DateTime endDate,
            int? excludeActivityId = null)
        {
            return await _context.Activities.AnyAsync(a =>
                a.ModuleId == moduleId &&
                (excludeActivityId == null ||
                 a.Id != excludeActivityId.Value) &&
                a.StartDate < endDate &&
                a.EndDate > startDate);
        }

        public async Task<bool> IsWithinModulePeriodAsync(
            int moduleId,
            DateTime startDate,
            DateTime endDate)
        {
            return await _context.Modules.AnyAsync(m =>
                m.Id == moduleId &&
                startDate >= m.StartDate &&
                endDate <= m.EndDate);
        }

        public async Task<IPagedList<StudentAssignmentReadModel>> GetStudentAssignmentsAsync(
        string userId,
        QueryParameters queryParameters,
        bool trackChanges = false)
        {
            var now = DateTime.UtcNow;

           var query = _context.Activities
           .Where(a =>
           a.ActionType.Name == "Assignment" &&
           a.Module != null &&
           a.Module.Course.ApplicationUsers
               .Any(u => u.Id == userId));

            if (!trackChanges)
            {
                query = query.AsNoTracking();
            }

            var assignments = query.Select(a => new
            {
                Activity = a,

                Submission = _context.Submissions
                    .Where(s =>
                        s.ActivityId == a.Id &&
                        s.UserId == userId)
                    .OrderByDescending(s => s.SubmittedAt)
                    .FirstOrDefault()
            });

            var result = assignments.Select(x => new StudentAssignmentReadModel
            {
                Id = x.Activity.Id,
                Name = x.Activity.Title,
                Description = x.Activity.Description,
                Deadline = x.Activity.EndDate,

                Status = x.Submission == null
                    ? (x.Activity.EndDate < now
                        ? "Overdue"
                        : "NotSubmitted")
                    : "Submitted",

                HasFeedback = x.Submission != null &&
                    _context.Feedbacks.Any(f =>
                        f.SubmissionId == x.Submission.Id),

                Comment = x.Submission == null
                    ? null
                    : _context.Feedbacks
                        .Where(f => f.SubmissionId == x.Submission.Id)
                        .OrderByDescending(f => f.CreatedAt)
                        .Select(f => f.Comment)
                        .FirstOrDefault(),

                Grade = x.Submission == null
                    ? null
                    : _context.Feedbacks
                        .Where(f => f.SubmissionId == x.Submission.Id)
                        .OrderByDescending(f => f.CreatedAt)
                        .Select(f => (decimal?)f.Grade)
                        .FirstOrDefault(),

                CreatedAt = x.Submission == null
                    ? null
                    : _context.Feedbacks
                        .Where(f => f.SubmissionId == x.Submission.Id)
                        .OrderByDescending(f => f.CreatedAt)
                        .Select(f => (DateTime?)f.CreatedAt)
                        .FirstOrDefault()
            });

            return await result.ToPagedListAsync(queryParameters);
        }
    }
}
