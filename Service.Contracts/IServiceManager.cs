namespace Service.Contracts;

public interface IServiceManager
{
    IAuthService AuthService { get; }
    ICourseService CourseService { get; }
    IUserService UserService { get; }
    INotificationService NotificationService { get; }
    IModuleService ModuleService { get; }
    IDocumentService DocumentService { get; }
    ISubmissionService SubmissionService { get; }
    IFeedbackService FeedbackService { get; }

    IDashboardService DashboardService { get; }
    IActionTypeService ActionTypeService { get; }
    IActivityService ActivityService { get; }
}
