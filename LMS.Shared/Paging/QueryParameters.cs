using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LMS.Shared.Paging
{
    public sealed record QueryParameters : IValidatableObject
    {
        private const int _maxPageSize = 100;

        [Range(1, int.MaxValue, ErrorMessage = "{0} must be at least {1}.")]
        public int PageNumber { get; init; } = 1;

        [Range(1, _maxPageSize, ErrorMessage = "{0} must be between {1} and {2}.")]
        public int PageSize { get; init; } = 10;

        [MaxLength(100)]
        public string? SearchTerm { get; init; }

        public int GetOffset => checked((PageNumber - 1) * PageSize);

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if ((long)(PageNumber - 1) * PageSize > int.MaxValue)
            {
                yield return new ValidationResult(
                    "The requested page is too large.",
                    [nameof(PageNumber), nameof(PageSize)]);
            }
        }
    }
}
