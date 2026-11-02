namespace LMS.Shared.DTOs.UserDtos;

public record CourseParticipantDto
{
    // Only filled in for teachers, so students do not see their classmates' ids
    public string? Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
