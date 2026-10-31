namespace LMS.Shared.DTOs.DocumentDtos;

public sealed record DocumentDto
(
    int Id,
    string FileName,
    string ContentType,
    long Size,
    DateTime UploadedAt,
    int? CourseId,
    int? ModuleId
);
