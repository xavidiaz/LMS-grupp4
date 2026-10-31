using LMS.Shared.DTOs.DocumentDtos;

namespace Service.Contracts;

public interface IDocumentService
{
    Task<IEnumerable<DocumentDto>> GetByCourseIdAsync(int courseId);
    Task<IEnumerable<DocumentDto>> GetByModuleIdAsync(int moduleId);
    Task<DocumentDto?> GetDocumentAsync(int id);
    Task<DocumentDto> UploadAsync(UploadDocumentDto document, Stream content, string userId);
    Task<DocumentFileDto?> DownloadAsync(int id);
    Task<bool> DeleteDocumentAsync(int id);
    Task<bool> IsInCourseAsync(int courseId, int documentId);
}

