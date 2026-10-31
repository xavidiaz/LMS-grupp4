using LMS.Shared.DTOs.FeedbackDtos;

namespace Service.Contracts;

public interface IFeedbackService
{
    Task<FeedbackDto> CreateAsync(int submissionId, CreateFeedbackDto dto, string teacherId);
    Task<IEnumerable<FeedbackDto>> GetBySubmissionIdAsync(int submissionId);
    Task<IEnumerable<FeedbackDto>> GetMyFeedbackAsync(int submissionId, string studentId);
}
