using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Models.Entities;

public class Submission
{
    public int Id { get; set; }

    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;

    // US13 - Link submission to an assignment activity
    public int? ActivityId { get; set; }

    public Activity? Activity { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public DateTime SubmittedAt { get; set; }

    public SubmissionStatus Status { get; set; }
}
