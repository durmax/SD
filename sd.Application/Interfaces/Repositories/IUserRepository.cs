using sd.Shared;
using System.Linq.Expressions;

namespace sd.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<UserModel>> GetByCondition(Expression<Func<UserModel, bool>> expression);
    Task<UserModel> GetById(string id);
    Task<bool> Create(UserModel user);
    Task<bool> Update(UserModel newVer);
    Task<bool> Delete(string id);
}
