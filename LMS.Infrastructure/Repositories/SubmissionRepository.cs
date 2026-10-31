using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.ReadModels;
using LMS.Infrastructure.Data;
using LMS.Infrastructure.Extensions;
using LMS.Shared.Paging;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class SubmissionRepository(ApplicationDbContext context) : RepositoryBase<Submission>(context), ISubmissionRepository
{
    public async Task<Submission?> GetByIdAsync(int id) =>
        await FindByCondition(s => s.Id == id)
        .Include(s => s.Activity)
        .FirstOrDefaultAsync();

    public async Task<List<Submission>> GetByActivityAndUserAsync(int activityId, string userId) =>
        await FindByCondition(s => s.ActivityId == activityId && s.UserId == userId)
        .OrderByDescending(s => s.SubmittedAt)
        .ToListAsync();

    public async Task<IPagedList<TeacherSubmissionReadModel>> GetTeacherSubmissionsAsync(
           int courseId,
           int? activityId,
           string? status,
           QueryParameters parameters)
    {
        var now = DateTime.UtcNow;

        var students = context.Users
            .Where(u =>
                u.CourseId == courseId &&
                context.UserRoles.Any(ur =>
                    ur.UserId == u.Id &&
                    context.Roles.Any(r =>
                        r.Id == ur.RoleId &&
                        r.Name == "Student")));

        var activities = context.Activities
            .Where(a =>
                a.ModuleId.HasValue &&
                a.Module.CourseId == courseId &&
                a.ActionType.Name == "Assignment");

        if (activityId.HasValue)
        {
            activities = activities.Where(
                a => a.Id == activityId.Value);
        }

        var query =
            from activity in activities
            from student in students
            let submission = context.Submissions
                .Where(s =>
                    s.ActivityId == activity.Id &&
                    s.UserId == student.Id)
                .OrderByDescending(s => s.SubmittedAt)
                .Select(s => new
                {
                    s.Id,
                    s.SubmittedAt,
                    s.FileName,
                    s.Status
                })
                .FirstOrDefault()
            select new TeacherSubmissionReadModel
            {
                StudentId = student.Id,

                StudentName =
                    ((student.FirstName ?? "") + " " +
                     (student.LastName ?? "")).Trim(),

                StudentEmail = student.Email ?? "",

                ActivityId = activity.Id,
                ActivityTitle = activity.Title,
                Deadline = activity.EndDate,

                SubmissionId = submission == null
                    ? null
                    : (int?)submission.Id,

                SubmittedAt = submission == null
                    ? null
                    : (DateTime?)submission.SubmittedAt,

                FileName = submission == null
                    ? null
                    : submission.FileName,

                Status = submission == null
                    ? (activity.EndDate < now
                        ? "Overdue"
                        : "Missing")
                    : submission.Status.ToString()
            };

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        return await query
            .OrderBy(x => x.Deadline)
            .ThenBy(x => x.StudentName)
            .ToPagedListAsync(parameters);
    }
}
