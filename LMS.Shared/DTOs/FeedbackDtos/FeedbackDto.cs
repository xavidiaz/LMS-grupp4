namespace LMS.Shared.DTOs.FeedbackDtos;

public sealed record FeedbackDto(
       int Id,
       int SubmissionId,
       string Comment,
       decimal Grade,
       DateTime CreatedAt,
       string TeacherName
         );
