using Domain.Models.Entities;
using Domain.Models.ReadModels;
using LMS.Shared.Paging;

namespace Domain.Contracts;

public interface ISubmissionRepository : IRepositoryBase<Submission>
{
    Task<Submission?> GetByIdAsync(int id);
    Task<List<Submission>> GetByActivityAndUserAsync(int activityId, string userId);
    Task<IPagedList<TeacherSubmissionReadModel>> GetTeacherSubmissionsAsync(
            int courseId,
            int? activityId,
            string? status,
            QueryParameters parameters);
}
