namespace LMS.Shared.DTOs.SubmissionDtos;

public sealed record SubmissionDto(
        int Id,
        int? ActivityId,
        string FileName,
        DateTime SubmittedAt,
        string Status
        );
