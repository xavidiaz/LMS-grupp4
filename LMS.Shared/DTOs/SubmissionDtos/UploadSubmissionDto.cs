namespace LMS.Shared.DTOs.SubmissionDtos;

public record UploadSubmissionDto(
        string FileName,
        string ContentType,
        long Size,
        int ActivityId
        );
