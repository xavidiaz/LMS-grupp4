namespace LMS.Shared.DTOs.DocumentDtos;

public sealed record UploadDocumentDto
(
 string FileName,
 string ContentType,
 long Size,
 int? CourseId,
 int? ModuleId
 );

