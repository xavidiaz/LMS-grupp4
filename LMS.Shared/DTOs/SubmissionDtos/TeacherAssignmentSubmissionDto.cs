using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.DTOs.SubmissionDtos
{
    public sealed record TeacherAssignmentSubmissionDto(
    string StudentId,
    string StudentName,
    string StudentEmail,
    int ActivityId,
    string ActivityTitle,
    DateTime Deadline,
    int? SubmissionId,
    DateTime? SubmittedAt,
    string Status,
    string? FileName
);
}
