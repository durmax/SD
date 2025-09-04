using sd.Shared;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace sd.Api.Application_layer.Interfaces.Repositories;

public interface IUserConfigRepository
{
    Task<UserConfigModel> GetById(string id);
    Task<bool> Update(UserConfigModel userConfigModel);
    Task<IEnumerable<UserConfigModel>> GetByCondation(Expression<Func<UserConfigModel, bool>> expression);
}
