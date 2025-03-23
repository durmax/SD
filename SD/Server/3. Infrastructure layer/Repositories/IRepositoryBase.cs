using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace sd.Api.Infrastructure.Repositories
{
    public interface IRepositoryBase<T>
    {
        IQueryable<Task<T>> GetByCondation(Expression<Func<T,bool>> expression);
        Task<bool> Create(T entity);
        Task<bool> Update(T entity);
        Task<bool> Delete(T entity);
    }
}
