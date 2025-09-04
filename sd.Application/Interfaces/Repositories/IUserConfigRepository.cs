using sd.Shared;
using System.Linq.Expressions;

namespace sd.Application.Interfaces.Repositories;

public interface IUserConfigRepository
{
    Task<UserConfigModel> GetById(string id);
    Task<bool> Update(UserConfigModel userConfigModel);
    Task<IEnumerable<UserConfigModel>> GetByCondation(Expression<Func<UserConfigModel, bool>> expression);
}
