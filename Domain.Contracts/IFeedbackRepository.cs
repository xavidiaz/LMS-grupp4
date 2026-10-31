using Domain.Models.Entities;

namespace Domain.Contracts;

public interface IFeedbackRepository : IRepositoryBase<FeedBack>
{
    Task<IEnumerable<FeedBack>> GetBySubmissionIdAsync(int submissionId);
}
