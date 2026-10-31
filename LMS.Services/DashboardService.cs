
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using LMS.Shared.DTOs.CourseDtos;
using LMS.Shared.DTOs.DashboardDtos;
using LMS.Shared.DTOs.NotificationDtos;
using Microsoft.EntityFrameworkCore;
using Service.Contracts;

namespace LMS.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    // US12 - Student Dashboard
    public async Task<DashboardDto?> GetStudentDashboardAsync(string userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null || !user.CourseId.HasValue)
            return null;

        var courseId = user.CourseId.Value;
        var now = DateTime.Now;

        var course = await _context.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new CourseDto(
                c.Id,
                c.Name,
                c.Code,
                c.Description,
                c.StartDate,
                c.EndDate))
            .FirstOrDefaultAsync();

        if (course is null)
            return null;

        var moduleIds = await _context.Modules
            .AsNoTracking()
            .Where(m => m.CourseId == courseId)
            .Select(m => m.Id)
            .ToListAsync();

        var activityIds = await _context.Set<Activity>()
            .AsNoTracking()
            .Where(a =>
                a.ModuleId.HasValue &&
                moduleIds.Contains(a.ModuleId.Value))
            .Select(a => a.Id)
            .ToListAsync();

        var upcomingActivities = await _context.Set<Activity>()
            .AsNoTracking()
            .Where(a =>
                a.ModuleId.HasValue &&
                moduleIds.Contains(a.ModuleId.Value) &&
                a.EndDate >= now)
            .OrderBy(a => a.StartDate)
            .Take(5)
            .Select(a => new DashboardActivityDto
            {
                Id = a.Id,
                Title = a.Title,
                ActionTypeName = a.ActionType.Name,
                StartDate = a.StartDate,
                EndDate = a.EndDate
            })
            .ToListAsync();

        var upcomingDeadlines = await _context.Set<Activity>()
            .AsNoTracking()
            .Where(a =>
                a.ModuleId.HasValue &&
                moduleIds.Contains(a.ModuleId.Value) &&
                a.EndDate >= now)
            .OrderBy(a => a.EndDate)
            .Take(5)

            .Select(a => new DashboardActivityDto
            {
                Id = a.Id,
                Title = a.Title,
                ActionTypeName = a.ActionType.Name,
                StartDate = a.StartDate,
                EndDate = a.EndDate
            })
            .ToListAsync();

        var recentDocuments = await _context.Set<Document>()
            .AsNoTracking()
            .Where(d =>
                d.CourseId == courseId ||
                (d.ModuleId.HasValue &&
                 moduleIds.Contains(d.ModuleId.Value)) ||
                (d.ActivityId.HasValue &&
                 activityIds.Contains(d.ActivityId.Value)))
            .OrderByDescending(d => d.UploadAt)
            .Take(5)
            .Select(d => new DashboardDocumentDto
            {
                Id = d.Id,
                FileName = d.StoredFile.FileName,
                UploadAt = d.UploadAt
            })
            .ToListAsync();

        var recentFeedback = await _context.Set<FeedBack>()
            .AsNoTracking()
            .Where(f => f.Submission.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Take(5)
            .Select(f => new DashboardFeedbackDto
            {
                Id = f.Id,
                SubmissionId = f.SubmissionId,
                Comment = f.Comment,
                Grade = f.Grade,
                CreatedAt = f.CreatedAt
            })
            .ToListAsync();

        var recentNotifications = await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(5)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Message = n.Message,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead
            })
            .ToListAsync();

        return new DashboardDto
        {
            Course = course,
            UpcomingActivities = upcomingActivities,
            UpcomingDeadlines = upcomingDeadlines,
            RecentDocuments = recentDocuments,
            RecentFeedback = recentFeedback,
            RecentNotifications = recentNotifications
        };
    }

    // US13 - Teacher Dashboard
    public async Task<TeacherDashboardDto?> GetTeacherDashboardAsync(string userId)
    {
        // Verify that the user exists
        var teacher = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (teacher is null)
            return null;

        // Verify the Teacher role
        var isTeacher = await (
            from userRole in _context.UserRoles
            join role in _context.Roles
                on userRole.RoleId equals role.Id
            where userRole.UserId == userId
                  && role.Name == "Teacher"
            select userRole.UserId
        ).AnyAsync();

        if (!isTeacher)
            return null;

        var now = DateTime.Now;
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        // Find the Student role
        var studentRoleId = await _context.Roles
            .AsNoTracking()
            .Where(r => r.Name == "Student")
            .Select(r => r.Id)
            .FirstOrDefaultAsync();

        // Get active courses with student and module counts
        var currentCourses = await _context.Courses
            .AsNoTracking()
            .Where(c =>
                c.StartDate < tomorrow &&
                c.EndDate >= today)
            .OrderBy(c => c.Name)
            .Select(c => new TeacherCourseDto
            {
                CourseId = c.Id,
                Name = c.Name,
                StartDate = c.StartDate,
                EndDate = c.EndDate,

                StudentCount = c.ApplicationUsers.Count(u =>
                    _context.UserRoles.Any(ur =>
                        ur.UserId == u.Id &&
                        ur.RoleId == studentRoleId)),

                ModuleCount = c.Modules.Count()
            })
            .ToListAsync();

        var courseIds = currentCourses
            .Select(c => c.CourseId)
            .ToList();

        // Get upcoming activities
        var upcomingActivities = await _context.Activities
            .AsNoTracking()
            .Where(a =>
                a.ModuleId.HasValue &&
                courseIds.Contains(a.Module.CourseId) &&
                a.EndDate >= now)
            .OrderBy(a => a.StartDate)
            .Take(5)
            .Select(a => new TeacherActivityDto
            {
                ActivityId = a.Id,
                CourseId = a.Module.CourseId,
                CourseName = a.Module.Course.Name,
                Title = a.Title,
                ActivityType = a.ActionType.Name,
                StartDate = a.StartDate,
                EndDate = a.EndDate
            })
            .ToListAsync();

        // Get recent submissions
        var recentActivities = await _context.Submissions
            .AsNoTracking()
            .Where(s =>
                s.User.CourseId.HasValue &&
                courseIds.Contains(s.User.CourseId.Value))
            .OrderByDescending(s => s.SubmittedAt)
            .Take(5)
            .Select(s => new TeacherRecentActivityDto
            {
                CourseId = s.User.CourseId!.Value,
                Description = "Submission received: " + s.FileName,
                ActivityType = "Submission",
                CreatedAt = s.SubmittedAt
            })
            .ToListAsync();

        // Get submissions waiting for feedback
        var pendingFeedback = await _context.Submissions
            .AsNoTracking()
            .Where(s =>
                s.User.CourseId.HasValue &&
                courseIds.Contains(s.User.CourseId.Value) &&
                !_context.Feedbacks.Any(f =>
                    f.SubmissionId == s.Id))
            .OrderBy(s => s.SubmittedAt)
            .Take(10)
            .Select(s => new TeacherPendingFeedbackDto
            {
                SubmissionId = s.Id,
                CourseId = s.User.CourseId!.Value,
                StudentName =
                    (s.User.FirstName ?? "") + " " +
                    (s.User.LastName ?? ""),
                AssignmentTitle = s.Activity != null
                    ? s.Activity.Title
                    : "Unlinked submission",
                SubmittedAt = s.SubmittedAt
            })
            .ToListAsync();

        // Get assignment activities for active courses
        var assignments = await _context.Activities
            .AsNoTracking()
            .Where(a =>
                a.ModuleId.HasValue &&
                courseIds.Contains(a.Module.CourseId) &&
                a.ActionType.Name == "Assignment")
            .Select(a => new
            {
                ActivityId = a.Id,
                CourseId = a.Module.CourseId,
                Title = a.Title,
                Deadline = a.EndDate
            })
            .ToListAsync();

        var assignmentIds = assignments
            .Select(a => a.ActivityId)
            .ToList();

        // Get students enrolled in active courses
        var students = await _context.Users
            .AsNoTracking()
            .Where(u =>
                u.CourseId.HasValue &&
                courseIds.Contains(u.CourseId.Value) &&
                _context.UserRoles.Any(ur =>
                    ur.UserId == u.Id &&
                    ur.RoleId == studentRoleId))
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                CourseId = u.CourseId!.Value
            })
            .ToListAsync();

        // Get submissions linked to assignments
        var submissions = await _context.Submissions
            .AsNoTracking()
            .Where(s =>
                s.ActivityId.HasValue &&
                assignmentIds.Contains(s.ActivityId.Value))
            .Select(s => new
            {
                s.Id,
                s.UserId,
                s.ActivityId,
                s.SubmittedAt
            })
            .ToListAsync();

        var submissionIds = submissions
            .Select(s => s.Id)
            .ToList();

        // Find submissions that already have feedback
        var reviewedSubmissionIds = await _context.Feedbacks
            .AsNoTracking()
            .Where(f => submissionIds.Contains(f.SubmissionId))
            .Select(f => f.SubmissionId)
            .ToListAsync();

        // Find new, late and missing assignments
        var submissionsNeedingAttention =
            new List<TeacherSubmissionDto>();

        foreach (var assignment in assignments)
        {
            // Only students belonging to this assignment's course
            var courseStudents = students
                .Where(s => s.CourseId == assignment.CourseId);

            foreach (var student in courseStudents)
            {
                var submission = submissions
                    .Where(s =>
                        s.ActivityId == assignment.ActivityId &&
                        s.UserId == student.Id)
                    .OrderByDescending(s => s.SubmittedAt)
                    .FirstOrDefault();

                var studentName =
                    ((student.FirstName ?? "") + " " +
                     (student.LastName ?? "")).Trim();

                if (submission is null)
                {
                    // Missing: deadline passed, no linked submission
                    if (assignment.Deadline < now)
                    {
                        submissionsNeedingAttention.Add(
                            new TeacherSubmissionDto
                            {
                                SubmissionId = null,
                                ActivityId = assignment.ActivityId,
                                CourseId = assignment.CourseId,
                                StudentId = student.Id,
                                StudentName = studentName,
                                AssignmentTitle = assignment.Title,
                                Deadline = assignment.Deadline,
                                Status = "Missing"
                            });
                    }

                    continue;
                }

                var status = string.Empty;

                if (submission.SubmittedAt > assignment.Deadline)
                {
                    status = "Late";
                }
                else if (!reviewedSubmissionIds.Contains(submission.Id))
                {
                    status = "New";
                }

                if (!string.IsNullOrEmpty(status))
                {
                    submissionsNeedingAttention.Add(
                        new TeacherSubmissionDto
                        {
                            SubmissionId = submission.Id,
                            ActivityId = assignment.ActivityId,
                            CourseId = assignment.CourseId,
                            StudentId = student.Id,
                            StudentName = studentName,
                            AssignmentTitle = assignment.Title,
                            Deadline = assignment.Deadline,
                            Status = status
                        });
                }
            }
        }

        // Return teacher dashboard data
        return new TeacherDashboardDto
        {
            CurrentCourses = currentCourses,
            UpcomingActivities = upcomingActivities,
            RecentActivities = recentActivities,
            PendingFeedback = pendingFeedback,

            SubmissionsNeedingAttention = submissionsNeedingAttention
                .OrderBy(s =>
                    s.Status == "New" ? 0 :
                    s.Status == "Late" ? 1 : 2)
                .ThenBy(s => s.Deadline)
                .Take(10)
                .ToList()
        };
    }
}
