using sd.Shared;
using System.Linq.Expressions;

namespace sd.Application.Interfaces.Repositories;
public interface IOtherPageRepository
{
    Task<IEnumerable<OtherPageModel>> GetByCondation(Expression<Func<OtherPageModel, bool>> expression);
    Task<OtherPageModel> GetById(string id);
    Task<bool> Create(OtherPageModel otherPage);
    Task<bool> Update(OtherPageModel newOtherPage);
    Task<bool> Delete(string id);
}
