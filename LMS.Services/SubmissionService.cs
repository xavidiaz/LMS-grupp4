using AutoMapper;
using Domain.Contracts;
using Domain.Models.Entities;
using Domain.Models.Exceptions;
using LMS.Shared.DTOs.SubmissionDtos;
using LMS.Shared.Paging;
using LMS.Shared.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Service.Contracts;

namespace LMS.Services;

public class SubmissionService(IUnitOfWork uow, IMapper mapper, IConfiguration configuration, UserManager<ApplicationUser> userManager) : ISubmissionService
{
    private readonly string _storageRoot = configuration["FileStorage:RootPath"] ?? Path.Combine(AppContext.BaseDirectory, "uploads");

    public async Task<SubmissionDto> SubmitAsync(UploadSubmissionDto dto, Stream content, string userId)
    {
        var error = FileRules.Validate(dto.FileName, dto.Size);
        if (error is not null) throw new InvalidFileException(error);

        // The student may only submit to an activity in their own course
        var activity = await uow.ActivityRepository.GetByIdWithModuleAsync(dto.ActivityId)
            ?? throw new NotFoundException($"Activity {dto.ActivityId} not found.");
        var user = await uow.UserRepsoitory.GetUserByIdAsync(userId)
            ?? throw new NotFoundException($"User {userId} not found.");

        if (activity.Module.CourseId != user.CourseId)
            throw new NotFoundException($"Activity {dto.ActivityId} not found.");

        var extension = Path.GetExtension(dto.FileName).ToLowerInvariant();
        var storageKey = $"{Guid.NewGuid()}{extension}";

        Directory.CreateDirectory(_storageRoot);
        await using (var fileStream = File.Create(Path.Combine(_storageRoot, storageKey)))
        {
            await content.CopyToAsync(fileStream);
        }

        var submittedAt = DateTime.UtcNow;
        var submission = new Submission
        {
            ActivityId = dto.ActivityId,
            UserId = userId,
            FileName = Path.GetFileName(dto.FileName),
            FilePath = storageKey,
            SubmittedAt = submittedAt,
            Status = submittedAt > activity.EndDate ? SubmissionStatus.Late : SubmissionStatus.Submitted
        };

        uow.SubmissionRepository.Create(submission);
        await uow.CompleteAsync();

        return mapper.Map<SubmissionDto>(submission);
    }

    public async Task<List<SubmissionDto>> GetMySubmissionsAsync(int activityId, string userId) =>
        mapper.Map<List<SubmissionDto>>(
                await uow.SubmissionRepository.GetByActivityAndUserAsync(activityId, userId));
    public async Task<PagedResponse<TeacherAssignmentSubmissionDto>> GetTeacherSubmissionsAsync(
        string teacherId,
        int? activityId,
        string? status,
        QueryParameters parameters)
    {
        var teacher = await userManager.FindByIdAsync(teacherId);

        if (teacher is null ||
            !await userManager.IsInRoleAsync(teacher, "Teacher"))
        {
            throw new TokenValidationException(
                "Only teachers can view submissions.");
        }

        // CourseId identifies the teacher's authorized course.
        if (teacher.CourseId is not int courseId || courseId <= 0)
        {
            throw new NotFoundException(
                "Could not load submissions", "Could not Load");
        }

        var allowedStatuses = new[]
        {
        "Submitted",
        "Late",
        "Missing",
        "Overdue"
    };

        if (!string.IsNullOrWhiteSpace(status))
        {
            status = allowedStatuses.FirstOrDefault(s =>
                string.Equals(
                    s,
                    status,
                    StringComparison.OrdinalIgnoreCase));

            if (status is null)
            {
                throw new InvalidSubmissionStatusException(
                      "Invalid submission status.");
            }
        }

        var result = await uow.SubmissionRepository
        .GetTeacherSubmissionsAsync(
            courseId,
            activityId,
            status,
            parameters);

        return mapper.Map<
            PagedResponse<TeacherAssignmentSubmissionDto>>(result);
    }
    public async Task<(Stream Content, string FileName)?> DownloadSubmissionAsync(
        int submissionId,
        string teacherId)
    {
        var teacher = await userManager.FindByIdAsync(teacherId);

        if (teacher is null ||
            !await userManager.IsInRoleAsync(teacher, "Teacher"))
        {
            throw new UnauthorizedAccessException(
                "Only teachers can download submissions.");
        }

        var submission =
            await uow.SubmissionRepository.GetByIdAsync(submissionId);

        if (submission is null ||
            !submission.ActivityId.HasValue)
        {
            return null;
        }

        var activity =
            await uow.ActivityRepository.GetByIdWithModuleAsync(
                submission.ActivityId.Value);

        if (activity?.Module is null)
        {
            return null;
        }

        if (teacher.CourseId != activity.Module.CourseId)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to download this file.");
        }

        var root = Path.GetFullPath(_storageRoot);

        var fullPath = Path.GetFullPath(
            Path.Combine(root, submission.FilePath));

        var rootWithSeparator =
            root.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(
            rootWithSeparator,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Invalid file storage path.");
        }

        if (!File.Exists(fullPath))
        {
            return null;
        }

        var stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        return (
            stream,
            Path.GetFileName(submission.FileName));
    }

}
