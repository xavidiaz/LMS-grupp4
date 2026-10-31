using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Contracts
{
    public interface IRepositoryBase<T> where T : class
    {
        void Create(T entity);
        void Delete(T entity);
    }
}
