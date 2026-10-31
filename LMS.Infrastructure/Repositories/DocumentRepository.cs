using Domain.Contracts;
using Domain.Models.Entities;
using LMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Repositories;

public class DocumentRepository(ApplicationDbContext context) : RepositoryBase<Document>(context), IDocumentRepository
{

    public async Task<Document?> GetByIdAsync(int id) =>
        await FindByCondition(d => d.Id == id)
        .Include(d => d.StoredFile)
        .Include(d => d.Module)
        .FirstOrDefaultAsync();

    public async Task<IEnumerable<Document>> GetByCourseIdAsync(int courseId) =>
        await FindByCondition(d =>
                d.CourseId == courseId ||
                (d.Module != null && d.Module.CourseId == courseId))
        .Include(d => d.StoredFile)
        .OrderByDescending(d => d.UploadAt)
        .ToListAsync();

    public async Task<IEnumerable<Document>> GetByModuleIdAsync(int moduleId) =>
        await FindByCondition(d => d.ModuleId == moduleId)
        .Include(d => d.StoredFile)
        .OrderByDescending(d => d.UploadAt)
        .ToListAsync();

    public void DeleteStoredFile(StoredFile file) => context.StoredFiles.Remove(file);

    public async Task<bool> IsInCourseAsync(int courseId, int moduleId)
    {
        return await context.Documents.AnyAsync(d => d.Id == moduleId && (d.CourseId == courseId || d.Module.CourseId == courseId));
    }
}

