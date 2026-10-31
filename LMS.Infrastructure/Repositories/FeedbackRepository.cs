using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class FeedbackRepository(ApplicationDbContext context) : RepositoryBase<FeedBack>(context), IFeedbackRepository
{
    public async Task<IEnumerable<FeedBack>> GetBySubmissionIdAsync(int submissionId) => await FindByCondition(f => f.SubmissionId == submissionId)
        .Include(f => f.User)
        .OrderByDescending(f => f.CreatedAt)
        .ToListAsync();
}
