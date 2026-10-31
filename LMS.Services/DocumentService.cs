using AutoMapper;
using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.DocumentDtos;
using Microsoft.Extensions.Configuration;
using Service.Contracts;

namespace LMS.Services;

public class DocumentService(IUnitOfWork uow, IMapper mapper, IConfiguration configuration) : IDocumentService
{
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    private static readonly string[] AllowedExtensions =
        [".pdf", ".docx", ".pptx", ".xlsx", ".txt", ".jpg", ".jpeg", ".png"];
    public readonly string _storageRoot = configuration["FileStorage:RootPath"] ?? Path.Combine(AppContext.BaseDirectory, "uploads");

    public async Task<IEnumerable<DocumentDto>> GetByCourseIdAsync(int courseId) =>
        mapper.Map<IEnumerable<DocumentDto>>(
                await uow.DocumentRepository.GetByCourseIdAsync(courseId));

    public async Task<IEnumerable<DocumentDto>> GetByModuleIdAsync(int moduleId) =>
        mapper.Map<IEnumerable<DocumentDto>>(
                await uow.DocumentRepository.GetByModuleIdAsync(moduleId));

    public async Task<DocumentDto?> GetDocumentAsync(int id) =>
        mapper.Map<DocumentDto>(
                await uow.DocumentRepository.GetByIdAsync(id));

    public async Task<DocumentDto> UploadAsync(UploadDocumentDto dto, Stream content, string userId)
    {
        ValidateFile(dto);

        var extension = Path.GetExtension(dto.FileName).ToLowerInvariant();
        var storageKey = $"{Guid.NewGuid()}{extension}";

        Directory.CreateDirectory(_storageRoot);
        await using (var fileStream = File.Create(Path.Combine(_storageRoot, storageKey)))
        {
            await content.CopyToAsync(fileStream);
        }

        var document = new Document
        {
            CourseId = dto.CourseId,
            ModuleId = dto.ModuleId,
            UploadAt = DateTime.UtcNow,
            UploadedById = userId,
            StoredFile = new StoredFile
            {
                FileName = Path.GetFileName(dto.FileName),
                ContentType = dto.ContentType,
                StorageKey = storageKey,
                Size = dto.Size
            }
        };

        uow.DocumentRepository.Create(document);
        await uow.CompleteAsync();

        return mapper.Map<DocumentDto>(document);
    }

    public async Task<DocumentFileDto?> DownloadAsync(int id)
    {
        var document = await uow.DocumentRepository.GetByIdAsync(id);
        if (document is null)
            return null;

        var path = Path.Combine(_storageRoot, document.StoredFile.StorageKey);
        if (!File.Exists(path))
            throw new NotFoundException($"File for document {id} is missing.");

        return new DocumentFileDto(
                File.OpenRead(path),
                document.StoredFile.FileName,
                document.StoredFile.ContentType);
    }

    public async Task<bool> DeleteDocumentAsync(int id)
    {
        var document = await uow.DocumentRepository.GetByIdAsync(id);
        if (document is null)
            return false;

        var path = Path.Combine(_storageRoot, document.StoredFile.StorageKey);

        // Removing the StoredFile also removes the Document (cascade delete)
        uow.DocumentRepository.DeleteStoredFile(document.StoredFile);
        await uow.CompleteAsync();

        // Delete the file on disk only after the database succeeded
        if (File.Exists(path))
            File.Delete(path);

        return true;
    }

    private static void ValidateFile(UploadDocumentDto dto)
    {
        if (dto.Size <= 0)
            throw new InvalidFileException("The file is empty.");
        if (dto.Size > MaxFileSize)
            throw new InvalidFileException($"The file is larger than {MaxFileSize} MB.");

        var extension = Path.GetExtension(dto.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidFileException($"File type '{extension}' is not allowed.");
    }

    public async Task<bool> IsInCourseAsync(int courseId, int moduleId)
    {
        return await uow.DocumentRepository.IsInCourseAsync(courseId, moduleId);
    }
}
