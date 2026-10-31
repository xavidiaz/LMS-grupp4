using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts
{
    public interface IPagedList<T>
    {
        IReadOnlyList<T> Items { get; }
        int TotalCount { get; }
    }
}
