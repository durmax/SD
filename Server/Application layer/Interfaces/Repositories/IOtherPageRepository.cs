using sd.Shared;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace sd.Api.Application_layer.Interfaces.Repositories;
public interface IOtherPageRepository
{
    Task<IEnumerable<OtherPageModel>> GetByCondation(Expression<Func<OtherPageModel, bool>> expression);
    Task<OtherPageModel> GetById(string id);
    Task<bool> Create(OtherPageModel otherPage);
    Task<bool> Update(OtherPageModel newOtherPage);
    Task<bool> Delete(string id);
}
