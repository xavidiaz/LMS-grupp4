using Domain.Contracts;
using LMS.Infrastructure.Data;

namespace LMS.Infrastructure.Repositories;

public class UnitOfWork(ApplicationDbContext context,
        Lazy<IUserRepository> userRepsoitory,
        Lazy<IModuleRepository> moduleRepository,
        Lazy<IActivityRepository> activityRepository,
        Lazy<IActionTypeRepository> actionTypeRepository,
        Lazy<IDocumentRepository> documentRepository,
        Lazy<ISubmissionRepository> submissionRepository,
        Lazy<IFeedbackRepository> feedbackRepository,
        Lazy<INotificationRepository> notificationRepository
        ) : IUnitOfWork
{
    private readonly ApplicationDbContext _context = context;
    private readonly Lazy<IUserRepository> _userRepsoitory = userRepsoitory;
    private readonly Lazy<IModuleRepository> _moduleRepository = moduleRepository;
    private readonly Lazy<IDocumentRepository> _documentRepository = documentRepository;
    private readonly Lazy<ISubmissionRepository> _submissionRepository = submissionRepository;
    private readonly Lazy<IFeedbackRepository> _feedbackRepository = feedbackRepository;
    private readonly Lazy<IActivityRepository> _activityRepository = activityRepository;
    private readonly Lazy<IActionTypeRepository> _actionTypeRepository = actionTypeRepository;
    private readonly Lazy<INotificationRepository> _notificationRepository = notificationRepository;
    public IUserRepository UserRepsoitory => _userRepsoitory.Value;
    public IModuleRepository ModuleRepository => _moduleRepository.Value;
    public IActivityRepository ActivityRepository => _activityRepository.Value;
    public IActionTypeRepository ActionTypeRepository => _actionTypeRepository.Value;
    public ISubmissionRepository SubmissionRepository => _submissionRepository.Value;
    public IFeedbackRepository FeedbackRepository => _feedbackRepository.Value;
    public IDocumentRepository DocumentRepository => _documentRepository.Value;
    public INotificationRepository NotificationRepository => _notificationRepository.Value;

    public async Task<int> CompleteAsync() => await _context.SaveChangesAsync();
}
