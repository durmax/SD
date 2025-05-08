using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace sd.Api.Infrastructure.Repositories
{
    public interface ICrudBase<T>
    {
        Task<IEnumerable<T>> GetByCondation(Expression<Func<T,bool>> expression);
        Task<bool> Create(T entity);
        Task<bool> Update(T entity);
        Task<bool> Delete(string id);
    }
}
