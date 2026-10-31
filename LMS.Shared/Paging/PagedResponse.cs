using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Shared.Paging
{
    public sealed record PagedResponse<T>(IEnumerable<T> Items, int PageNumber, int PageSize, int TotalCount)
    {
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPreviousPage => PageNumber > 1;

        public bool HasNextPage => PageNumber < TotalPages;
    }

}
