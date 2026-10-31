using System.Net.NetworkInformation;
using Service.Contracts;

namespace LMS.Services;

public class ServiceManager(
        Lazy<IAuthService> authService,
        Lazy<IUserService> userService,
        Lazy<INotificationService> notificationService,
        Lazy<ICourseService> courseService,
        Lazy<IModuleService> moduleService,
        Lazy<IDocumentService> documentService,
        Lazy<ISubmissionService> submissionService,
        Lazy<IFeedbackService> feedbackService,
        Lazy<IActivityService> activityService,
        Lazy<IActionTypeService> actionTypeService,

        IDashboardService dashboardService) : IServiceManager
{
    private readonly Lazy<IAuthService> _authService = authService;
    private readonly Lazy<IUserService> _userService = userService;
    private readonly Lazy<INotificationService> _notificationService = notificationService;
    private readonly Lazy<ICourseService> _courseService = courseService;
    private readonly Lazy<IModuleService> _moduleService = moduleService;
    private readonly Lazy<IDocumentService> _documentService = documentService;
    private readonly Lazy<ISubmissionService> _submissionService = submissionService;
    private readonly Lazy<IFeedbackService> _feedbackService = feedbackService;
    private readonly Lazy<IActionTypeService> _actionTypeService = actionTypeService;
    private readonly Lazy<IActivityService> _activityService = activityService; 
    private readonly IDashboardService _dashboardService = dashboardService;

    public IAuthService AuthService => _authService.Value;
    public IUserService UserService => _userService.Value;
    public INotificationService NotificationService => _notificationService.Value;
    public ICourseService CourseService => _courseService.Value;
    public IModuleService ModuleService => _moduleService.Value;
    public IDocumentService DocumentService => _documentService.Value;
    public ISubmissionService SubmissionService => _submissionService.Value;
    public IDashboardService DashboardService => _dashboardService;
    public IFeedbackService FeedbackService => _feedbackService.Value;
    public IActivityService ActivityService => _activityService.Value;
   public IActionTypeService ActionTypeService => _actionTypeService.Value; 
}
