using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.ReadModels;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Extensions;
using LMS.Shared.Paging;
using Microsoft.EntityFrameworkCore;


namespace LMS.Infrastructure.Repositories
{

    public class UserRepository : RepositoryBase<ApplicationUser>, IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(
            string email,
            bool includeCourse = false,
            bool trackChanges = false)
        {
            IQueryable<ApplicationUser> query = _context.Users;

            if (includeCourse)
            {
                query = query.Include(u => u.Course);
            }

            return await query
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(
            string id,
            bool includeCourse = false,
            bool trackChanges = false)
        {
            IQueryable<ApplicationUser> query = _context.Users;

            if (includeCourse)
            {
                query = query.Include(u => u.Course);
            }

            return await query
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IPagedList<ApplicationUser>> GetAllUsersAsync(
            QueryParameters parameters,
            bool includeCourse = false,
            bool trackChanges = false)
        {
            IQueryable<ApplicationUser> query = GetAllUsersQuery(includeCourse, trackChanges);

            return await query.ToPagedListAsync(parameters);
        }
        private IQueryable<ApplicationUser> GetAllUsersQuery(bool includeCourse, bool trackChanges)
        {
            return includeCourse ? FindAll(trackChanges)
                                        .Include(u => u.Course)
                                        :
                                       FindAll(trackChanges);
        }
        public async Task<IPagedList<UserWithRole>> GetAllUsersWithRolesAsync(
        QueryParameters parameters)
        {
            var query = _context.Users
            .Include(u => u.Course)
            .Join(
            _context.UserRoles,
        user => user.Id,
        userRole => userRole.UserId,
        (user, userRole) => new { user, userRole }
                 )
         .Join(
            _context.Roles,
        x => x.userRole.RoleId,
        role => role.Id,
        (x, role) => new UserWithRole
        {
            User = x.user,
            Role = role.Name
        });

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                var searchTerm = parameters.SearchTerm.Trim();

                query = query.Where(x =>
                    (x.User.FirstName ?? "").Contains(searchTerm) ||
                    (x.User.LastName ?? "").Contains(searchTerm) ||
                    (x.User.Email ?? "").Contains(searchTerm));
            }
            return await query
               .OrderBy(x => x.User.LastName)
               .ThenBy(x => x.User.FirstName)
               .ThenBy(x => x.User.Id)
               .ToPagedListAsync(parameters);
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersByCourseIdAsync(int courseId, bool trackChanges = false) =>
            await FindByCondition(u =>
                    u.CourseId == courseId, trackChanges)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ToListAsync();

    }

}
