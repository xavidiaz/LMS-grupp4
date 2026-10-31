using System.ComponentModel.DataAnnotations;

namespace LMS.Shared.DTOs.FeedbackDtos;

public sealed record CreateFeedbackDto
{
    [Required(ErrorMessage = "Comment is Required.")]
    [MaxLength(2000)]
    public string Comment { get; init; } = string.Empty;

    [Range(0, 10, ErrorMessage = "Grade must be between 0 and 10.")]
    public decimal Grade { get; init; }
}
