
namespace LMS.Shared.DTOs.DashboardDtos;

public class TeacherDashboardDto
{
    public List<TeacherCourseDto> CurrentCourses { get; set; } = [];

    public List<TeacherActivityDto> UpcomingActivities { get; set; } = [];

    public List<TeacherRecentActivityDto> RecentActivities { get; set; } = [];

    public List<TeacherSubmissionDto> SubmissionsNeedingAttention { get; set; } = [];

    public List<TeacherPendingFeedbackDto> PendingFeedback { get; set; } = [];
}

public class TeacherCourseDto
{
    public int CourseId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int StudentCount { get; set; }
    public int ModuleCount { get; set; }
}

public class TeacherActivityDto
{
    public int ActivityId { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class TeacherRecentActivityDto
{
    public int CourseId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ActivityType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class TeacherSubmissionDto
{
    public int? SubmissionId { get; set; }
    public int ActivityId { get; set; }
    public int CourseId { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string AssignmentTitle { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class TeacherPendingFeedbackDto
{
    public int SubmissionId { get; set; }
    public int CourseId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string AssignmentTitle { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}
