using Domain.Models.Entities;

namespace Domain.Contracts;

public interface IDocumentRepository : IRepositoryBase<Document>
{
    Task<Document?> GetByIdAsync(int id);
    Task<IEnumerable<Document>> GetByCourseIdAsync(int courseId);
    Task<IEnumerable<Document>> GetByModuleIdAsync(int moduleId);
    Task<IEnumerable<Document>> GetByActivityIdAsync(int activityId);
    void DeleteStoredFile(StoredFile name);
    Task<bool> IsInCourseAsync(int courseId, int documentId);
}
