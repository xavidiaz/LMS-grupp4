using Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace LMS.Infrastructure.Paging
{
    public class PagedList<T> : IPagedList<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int TotalCount { get; }

        public PagedList(IReadOnlyList<T> items, int totalCount)
        {
            Items = items;
            TotalCount = totalCount;
        }
    }
}
