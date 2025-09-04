using sd.Shared;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace sd.Api.Application_layer.Interfaces.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<UserModel>> GetByCondation(Expression<Func<UserModel, bool>> expression);
    Task<UserModel> GetById(string id);
    Task<bool> Create(UserModel user);
    Task<bool> Update(UserModel newVer);
    Task<bool> Delete(string id);
}
