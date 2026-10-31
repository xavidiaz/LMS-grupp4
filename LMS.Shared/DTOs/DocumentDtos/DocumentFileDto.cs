namespace LMS.Shared.DTOs.DocumentDtos;

public sealed record DocumentFileDto
(
 Stream Content,
 string FileName,
 string ContentType
 );


