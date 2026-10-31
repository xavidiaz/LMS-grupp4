using LMS.Shared.DTOs.CourseDtos;
using LMS.Shared.DTOs.NotificationDtos;

namespace LMS.Shared.DTOs.DashboardDtos;

public class DashboardDto
{
    public CourseDto? Course { get; set; }

    public List<DashboardActivityDto> UpcomingActivities { get; set; } = [];
    public List<DashboardActivityDto> UpcomingDeadlines { get; set; } = [];

    public List<DashboardAssignmentDto> AssignmentsNeedingAttention { get; set; } = [];

    public List<DashboardDocumentDto> RecentDocuments { get; set; } = [];
    public List<DashboardFeedbackDto> RecentFeedback { get; set; } = [];
    public List<NotificationDto> RecentNotifications { get; set; } = [];
}

public class DashboardActivityDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ActionTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class DashboardAssignmentDto
{
    public int ActivityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime Deadline { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class DashboardDocumentDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadAt { get; set; }
}

public class DashboardFeedbackDto
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public decimal Grade { get; set; }
    public DateTime CreatedAt { get; set; }
}